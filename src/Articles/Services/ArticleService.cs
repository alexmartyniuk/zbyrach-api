using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Zbyrach.Api.Migrations;
using Microsoft.EntityFrameworkCore;

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
    }
}