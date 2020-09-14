using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Zbyrach.Api.Account;
using Zbyrach.Api.Migrations;
using Zbyrach.Api.Tags;

namespace Zbyrach.Api.Articles
{
    public class ArticleService
    {
        private readonly ApplicationContext _db;
        private readonly UsersService _usersService;

        public ArticleService(ApplicationContext db, UsersService usersService)
        {
            _db = db;
            _usersService = usersService;
        }

        public Task<List<Article>> GetByUrls(IEnumerable<string> urls)
        {
            return _db.Articles
                .Where(a => urls.Contains(a.Url))
                .ToListAsync();
        }

        public async Task<Article> SaveOne(Article article)
        {
            _db.Articles.Add(article);
            await _db.SaveChangesAsync();

            return await _db.Articles.FindAsync(article.Id);
        }

        public Task SetStatus(Article article, IEnumerable<User> users, ArticleStatus status)
        {
            return SetStatus(new List<Article> { article }, users, status);
        }

        public Task MarkAsSent(IEnumerable<Article> articles, User user)
        {
            return SetStatus(articles, new List<User> { user }, ArticleStatus.Sent);
        }

        public Task MarkAsRead(Article article, User user)
        {
            return SetStatus(new List<Article> { article }, new List<User> { user }, ArticleStatus.Read);
        }

        public virtual async Task<Dictionary<User, DateTime>> GetLastMailSentDateByUsers()
        {
            var groupedUsers = await _db.ArticleUsers
                .Where(au => au.Status == ArticleStatus.Sent)
                .GroupBy(c => c.User.Id)
                .Select(g => new
                {
                    userId = g.Key,
                    maxSentAt = g.Max(x => x.SentAt)
                })
                .ToListAsync();

            var userIds = groupedUsers
                .Select(gu => gu.userId)
                .ToList();

            var users = await _db.Users
                .Where(u => userIds.Contains(u.Id))
                .ToDictionaryAsync(u => u.Id, u => u);

            return groupedUsers.ToDictionary(g => users[g.userId], g => g.maxSentAt);
        }        

        public ValueTask<Article> GetById(long articleId)
        {
            return _db.Articles.FindAsync(articleId);
        } 

        public Task<Article> GetByTitleAndAuthorName(string title, string authorName)
        {
            return _db.Articles
                .SingleOrDefaultAsync(a => a.Title == title && a.AuthorName == authorName);
        }

        public async Task LinkWithTag(Article article, Tag tag)
        {
            var isAlreadyLinked = await _db
                .ArticleTags
                .AnyAsync(at => at.ArticleId == article.Id && at.TagId == tag.Id);

            if (isAlreadyLinked) return;

            _db.ArticleTags.Add(new ArticleTag
            {
                ArticleId = article.Id,
                TagId = tag.Id
            });
            await _db.SaveChangesAsync();
        }        

        public async Task<List<Article>> GetForSending(User user, long noMoreThan)
        {
            if (noMoreThan == 0) return new List<Article>();

            var result = await _db.ArticleUsers
                .Include(au => au.Article)
                .Where(au => au.UserId == user.Id && au.Status == ArticleStatus.New)
                .Select(au => au.Article)
                .OrderByDescending(a => a.LikesCount)
                .ThenByDescending(a => a.CommentsCount)
                .ThenByDescending(a => a.PublicatedAt)
                .Take((int)noMoreThan)
                .ToListAsync();

            return result;
        }

        public async Task<List<Article>> GetForReading(User user)
        {
            var tagIds = await _db.TagUsers
                .Where(tu => tu.UserId == user.Id)
                .Select(tu => tu.TagId)
                .ToListAsync();

            var articleIds = await _db.ArticleTags
                .Where(at => tagIds.Contains(at.TagId))
                .Select(at => at.ArticleId)
                .ToListAsync();

            var supportedLanguages = new List<string>
            {
                "English",
                "Ukrainian",
                "Russian",
                "en",
                "uk",
                "ru"
            };

            var articles = await _db.Articles
                .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
                .Where(a => articleIds.Contains(a.Id))
                .Where(a => supportedLanguages.Contains(a.Language))
                .OrderByDescending(a => a.PublicatedAt)
                .Take(20)
                .ToListAsync();

            return articles;
        }

        public async Task<List<ArticleUser>> GetArticleUsers(User user, IEnumerable<Article> articles)
        {
            var articleIds = articles.Select(a => a.Id).ToList();

            return await _db.ArticleUsers
                .Where(au => au.UserId == user.Id)
                .Where(au => articleIds.Contains(au.ArticleId))
                .ToListAsync();
        }

        public async Task<ArticleUser> GetArticleUser(User user, Article article)
        {
            return await _db.ArticleUsers
                .Where(au => au.UserId == user.Id)
                .Where(au => au.Id == article.Id)
                .SingleAsync();
        }

        public async Task<Article> SetFavorite(User user, Article article, bool favorite)
        {
            var articleUser = await _db
                .ArticleUsers
                .Where(au => au.ArticleId == article.Id)
                .Where(au => au.UserId == user.Id)
                .SingleOrDefaultAsync();

            articleUser.Favorite = favorite;

            await _db.SaveChangesAsync();

            return await _db.Articles
                .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
                .Where(a => a.Id == article.Id)                
                .SingleAsync();
        }

        public async Task SetReadLater(User user, Article article, bool readLater)
        {
            var articleUser = await _db
                .ArticleUsers
                .Where(au => au.ArticleId == article.Id)
                .Where(au => au.UserId == user.Id)
                .SingleOrDefaultAsync();

            articleUser.ReadLater = readLater;

            await _db.SaveChangesAsync();
        }

        private async Task SetStatus(IEnumerable<Article> articles, IEnumerable<User> users, ArticleStatus newStatus)
        {
            var articleIds = articles.Select(a => a.Id).ToList();
            var userIds = users.Select(u => u.Id).ToList();

            // 1. Update existing readings
            var existingReading = await _db
                .ArticleUsers
                .Where(au => articleIds.Contains(au.ArticleId) && userIds.Contains(au.UserId))
                .ToListAsync();
            existingReading.ForEach(reading =>
            {
                UpdateStatus(reading, newStatus);
            });
            _db.ArticleUsers.UpdateRange(existingReading);

            // 2. Add new readings
            var newArticleUserPairs = articleIds
                .Join(userIds,
                    a => true,
                    u => true,
                    (articleId, userId) => new { articleId, userId }
                )
                .Where(au => !existingReading.Any(r => r.ArticleId == au.articleId && r.UserId == au.userId));
            var newReadings = newArticleUserPairs
                .Select(p => new ArticleUser
                {
                    ArticleId = p.articleId,
                    UserId = p.userId
                })
                .ToList();
            newReadings.ForEach(reading =>
            {
                UpdateStatus(reading, newStatus);
            });
            await _db.ArticleUsers.AddRangeAsync(newReadings);

            // 3. Save changes
            await _db.SaveChangesAsync();
        }

        private void UpdateStatus(ArticleUser reading, ArticleStatus newStatus)
        {
            if (reading.Status >= newStatus)
                // cant downgrade status
                return;

            switch (newStatus)
            {
                case ArticleStatus.New:
                    reading.CreatedAt = DateTime.UtcNow;
                    break;
                case ArticleStatus.Sent:
                    reading.SentAt = DateTime.UtcNow;
                    break;
                case ArticleStatus.Read:
                    reading.ReadAt = DateTime.UtcNow;
                    break;
                default:
                    throw new Exception($"Unknown status: {newStatus}");
            }
            reading.Status = newStatus;
        }
    }
}