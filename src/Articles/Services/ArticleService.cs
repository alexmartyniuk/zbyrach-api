using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;
using Zbyrach.Api.Account;

namespace Zbyrach.Api.Articles
{
    public class ArticleService
    {
        private readonly ApplicationContext _db;

        public ArticleService(ApplicationContext db)
        {
            _db = db;
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

        public async Task SetStatus(IEnumerable<Article> articles, IEnumerable<User> users, ArticleStatus status)
        {
            var articleIds = articles.Select(a => a.Id).ToList();
            var userIds = users.Select(u => u.Id).ToList();

            // 1. Update existing readings
            var existingReading = await _db
                .ArticleUsers
                .Where(au => articleIds.Contains(au.ArticleId) && userIds.Contains(au.UserId))
                .ToListAsync();
            foreach (var reading in existingReading)
            {
                reading.Status = status;
            }
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
                    UserId = p.userId,
                    Status = status
                })
                .ToList();
            _db.ArticleUsers.AddRange(newReadings);

            // 3. Save changes
            await _db.SaveChangesAsync();
        }

        public async Task<Article> GetByExternalIdWithTags(string extenalId)
        {
            return await _db.Articles
                .Include(a => a.ArticleTags)
                .ThenInclude(at => at.Tag)
                .SingleOrDefaultAsync(a => a.ExternalId == extenalId);
        }

        public async Task UpdateOne(Article originalArticle)
        {
            _db.Articles.Update(originalArticle);
            await _db.SaveChangesAsync();
        }

        public async Task<List<Article>> GetAllForUserByTags(User user)
        {
            var tagIds = await _db.Tags
                .Where(t => t.TagUsers.Any(tu => tu.UserId == user.Id))
                .Select(t => t.Id)
                .ToListAsync();

            var result = await _db.Articles
                .Where(a => a.ArticleTags.Any(at => tagIds.Contains(at.TagId)))
                .ToListAsync();

            return result;
        }

        public async Task<List<Article>> GetNewForUser(User user)
        {
            var result = await _db.ArticleUsers
                .Include(au => au.Article)
                .Where(au => au.UserId == user.Id && au.Status == ArticleStatus.New)
                .Select(au => au.Article)
                .ToListAsync();

            return result;
        }
    }
}