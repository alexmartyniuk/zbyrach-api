using System;
using System.Collections.Generic;

namespace Zbyrach.Api.Mailing.Templates
{
    public class ArticlesModel
    {
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        public string DateTime { get; set; }
        public string ViewOnSiteUrl { get; set; }
        public string UnsubscribeUrl { get; set; }
        public List<ArticleModel> Articles { get; set; }
    }

    public class ArticleModel
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublicatedAt { get; set; }
        public string Url { get; set; }
        public string PdfUrl { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorPhoto { get; set; }
        public long CommentsCount { get; set; }
        public long LikesCount { get; set; }
        public string ReadTime { get; set; }
    }
}