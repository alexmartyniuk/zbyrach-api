using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using Zbyrach.Api.Account;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;

namespace Zbyrach.Api.Tags
{
    public class TagService
    {        private readonly ApplicationContext _db;
        private readonly TagsComparer _tagsComparer = new TagsComparer();

        public TagService(ApplicationContext db)
        {
            _db = db;
        }

        public Task<List<Tag>> GetByUser(User user)
        {
            var originalUser = _db.Users.Find(user.Id);
            return _db.Tags
                .Include(t => t.TagUsers)
                .Where(t => t.TagUsers.Any(tu => tu.UserId == originalUser.Id))
                .ToListAsync();
        }

        public async Task SetByUser(User user, IEnumerable<Tag> tags)
        {
            var originalUser = _db.Users.Find(user.Id);
            tags = await SaveTagsIfNeeded(tags);

            var tagsExisting = await _db
                .TagUsers
                .Include(tu => tu.Tag)
                .Where(tu => tu.UserId == originalUser.Id)
                .Select(tu => tu.Tag)
                .ToListAsync();

            var tagsToAdd = tags
                .Except(tagsExisting, _tagsComparer)
                .ToList();
            var tagToRemove = tagsExisting
                .Except(tags, _tagsComparer)
                .ToList();

            var tagUsersToAdd = tagsToAdd
                .Select(t => new TagUser
                {
                    TagId = t.Id,
                    Tag = t,
                    UserId = originalUser.Id,
                    User = originalUser
                })
                .ToList();
            _db.TagUsers.AddRange(tagUsersToAdd);

            var tagUsersToRemove = tagToRemove
                .Select(t => new TagUser
                {
                    TagId = t.Id,
                    Tag = t,
                    UserId = originalUser.Id,
                    User = originalUser
                })
                .ToList();
            _db.TagUsers.RemoveRange(tagUsersToRemove);

            await _db.SaveChangesAsync();
        }

        public async Task<long> GetTagsCountByUser(User user)
        {
            var originalUser = _db.Users.Find(user.Id);
            return await _db.Tags
                .Include(t => t.TagUsers)
                .Where(t => t.TagUsers.Any(tu => tu.UserId == originalUser.Id))
                .CountAsync();
        }

        public async Task<Dictionary<Tag, List<User>>> GetTagsWithUsers()
        {
            // TODO: take into account schedule settings for users        
            var users = await _db.Users
                .Include(u => u.TagUsers)
                .ThenInclude(tu => tu.Tag)
                .ToListAsync();
            var tags = users
                .SelectMany(u => u.TagUsers)
                .Select(tu => tu.Tag)
                .GroupBy(t => t)
                .ToDictionary(g => g.First(), g => new List<User>());

            foreach (var user in users)            
            {
                var userTags = user.TagUsers.Select(tg => tg.Tag);
                foreach (var userTag in userTags)
                {
                    tags[userTag].Add(user);
                }                
            }

            return tags;
        }

        private async Task<IEnumerable<Tag>> SaveTagsIfNeeded(IEnumerable<Tag> tags)
        {
            var tagNames = tags
                .Select(t => t.Name)
                .ToList();
            var tagsExisting = _db.Tags
                .Where(t => tagNames.Contains(t.Name))
                .ToList();
            var tagsToAdd = tags
                .Except(tagsExisting, _tagsComparer)
                .ToList();
            _db.Tags.AddRange(tagsToAdd);
            await _db.SaveChangesAsync();

            return await _db
                .Tags
                .Where(t => tagNames.Contains(t.Name))
                .ToListAsync();
        }
    }

    internal class TagsComparer : IEqualityComparer<Tag>
    {
        public bool Equals([AllowNull] Tag x, [AllowNull] Tag y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return x.Name.Equals(y.Name, StringComparison.OrdinalIgnoreCase);
        }

        public int GetHashCode([DisallowNull] Tag obj)
        {
            if (obj == null)
            {
                return 0;
            }

            return obj.Name.GetHashCode();
        }
    }
}