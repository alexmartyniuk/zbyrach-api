using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading.Tasks;
using MediumGrabber.Api.Account;
using MediumGrabber.Api.Migrations;
using Microsoft.EntityFrameworkCore;

namespace MediumGrabber.Api.Tags
{
    public class TagService
    {
        private readonly UsersService _userService;
        private readonly ApplicationContext _db;
        private readonly TagsComparer _tagsComparer = new TagsComparer();

        public TagService(UsersService userService, ApplicationContext db)
        {
            _userService = userService;
            _db = db;
        }

        public async Task<IEnumerable<Tag>> GetMyTags()
        {
            var currentUser = await _userService.GetCurrentUser();
            return await _db.Tags.Where(t => t.UserId == currentUser.Id).ToListAsync();
        }

        public async Task SetMyTags(IEnumerable<Tag> tags)
        {
            var currentUser = await _userService.GetCurrentUser();
            var tagsExisting = await _db.Tags.Where(t => t.UserId == currentUser.Id).ToListAsync();

            var tagsToAdd = tags
                .Except(tagsExisting, _tagsComparer)
                .ToList();
            var tagsToRemove = tagsExisting
                .Except(tags, _tagsComparer)
                .ToList();

            foreach (var tag in tagsToAdd)
            {
                tag.User = currentUser;
            }

            _db.Tags.AddRange(tagsToAdd);
            _db.Tags.RemoveRange(tagsToRemove);

            await _db.SaveChangesAsync();
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