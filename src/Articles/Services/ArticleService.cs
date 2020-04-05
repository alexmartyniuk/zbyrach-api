using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using MediumGrabber.Api.Migrations;
using Microsoft.EntityFrameworkCore;

namespace MediumGrabber.Api.Articles
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

        public async Task SaveOne(Article article)
        {
            throw new NotImplementedException();
        }

        public async Task<Article> GetByExternalIdWithTags(string id)
        {
            throw new NotImplementedException();
        }

        internal async Task UpdateOne(Article originalArticle)
        {
            throw new NotImplementedException();
        }
    }
}