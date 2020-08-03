using System;
using System.Collections.Generic;

namespace Zbyrach.Api.Articles
{
    public class ArticleDto
    {
        public long Id { get; set; }        
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublicatedAt { get; set; }                        
        public string IllustrationUrl { get; set; }
        public string OriginalUrl { get; set; }
        public string AuthorName { get; set; }
        public string AuthorPhoto { get; set; }
        public long CommentsCount { get; set; }
        public long LikesCount { get; set; }
        public string ReadTime { get; set; }  
        public List<string> Tags {get; set; }              
    }
}