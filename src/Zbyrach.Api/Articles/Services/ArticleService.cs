using System;
using System.Collections.Generic;
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
        private readonly PdfService _pdfService;

        public ArticleService(ApplicationContext db, UsersService usersService, PdfService pdfService)
        {
            _db = db;
            _usersService = usersService;
            _pdfService = pdfService;
        }

        public Task<List<Article>> GetByUrls(IEnumerable<string> urls)
        {
            return _db.Articles
                .Where(a => urls.Contains(a.Url))
                .ToListAsync();
        }

        public async Task MarkAsSent(IEnumerable<Article> articles, User user)
        {
            await SetStatus(articles, new List<User> { user }, ArticleStatus.Sent);
            await _db.SaveChangesAsync();
        }

        public async Task MarkAsRead(Article article, User user)
        {
            await SetStatus(new List<Article> { article }, new List<User> { user }, ArticleStatus.Read);
            await _db.SaveChangesAsync();
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

            return groupedUsers
                .ToDictionary(g => users[g.userId], g => g.maxSentAt);
        }

        public ValueTask<Article> FindById(long articleId)
        {
            return _db.Articles.FindAsync(articleId);
        }

        public async Task<List<Article>> GetLastSent(User user)
        {
            var lastSentAt = await _db.ArticleUsers
                .Where(au => au.Status == ArticleStatus.Sent)
                .Where(au => au.UserId == user.Id)
                .MaxAsync(au => au.SentAt);

            var result = await _db.ArticleUsers
                .Include(au => au.Article)
                .Where(au => au.UserId == user.Id && au.Status == ArticleStatus.Sent && au.SentAt == lastSentAt)
                .Select(au => au.Article)
                .OrderByDescending(a => a.LikesCount)
                .ThenByDescending(a => a.CommentsCount)
                .ThenByDescending(a => a.PublicatedAt)
                .ToListAsync();

            return result;
        }

        public async Task<Article> SaveArticle(Article newArticle, List<User> users, Tag tag)
        {
            var originalArticle = await _db.Articles
               .SingleOrDefaultAsync(a =>
                    a.Title == newArticle.Title &&
                    a.AuthorName == newArticle.AuthorName);

            if (originalArticle == null)
            {
                _db.Articles.Add(newArticle);
                await LinkWithTag(newArticle, tag);
                await _pdfService.QueueArticle(newArticle.Url);

                originalArticle = newArticle;
            }

            await SetStatus(new List<Article> { originalArticle }, users, ArticleStatus.New);

            await _db.SaveChangesAsync();

            return originalArticle;
        }

        public async Task<List<Article>> GetForSending(User user, long noMoreThan)
        {
            if (noMoreThan == 0)
            {
                return new List<Article>();
            }

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
            var newArticleUserPairs = articles
                .Join(users,
                    a => true,
                    u => true,
                    (article, user) => new { article, user }
                )
                .Where(au => !existingReading.Any(r => r.ArticleId == au.article.Id && r.UserId == au.user.Id));
            var newReadings = newArticleUserPairs
                .Select(p => new ArticleUser
                {
                    Article = p.article,
                    User = p.user
                })
                .ToList();
            newReadings.ForEach(reading =>
            {
                UpdateStatus(reading, newStatus);
            });
            await _db.ArticleUsers.AddRangeAsync(newReadings);
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

        private async Task LinkWithTag(Article article, Tag tag)
        {
            var isAlreadyLinked = await _db
                .ArticleTags
                .AnyAsync(at => at.ArticleId == article.Id && at.TagId == tag.Id);

            if (isAlreadyLinked) return;

            _db.ArticleTags.Add(new ArticleTag
            {
                Article = article,
                Tag = tag
            });
        }
    }
}