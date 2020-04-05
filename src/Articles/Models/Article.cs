using System;
using System.Collections.Generic;
using MediumGrabber.Api.Tags;

namespace MediumGrabber.Api.Articles
{
    public class Article
    {
        public long Id { get; set; }
        public string ExternalId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public DateTime PublicatedAt { get; set; }
        public DateTime FoundAt { get; set; }
        public string FileName { get; set; }
        public string Url { get; set; }
        public string IllustrationUrl { get; set; }
        public string AuthorName { get; set; }
        public string AuthorEmail { get; set; }
        public string AuthorPhoto { get; set; }
        public long CommentsCount { get; set; }
        public long LikesCount { get; set; }
        public string ReadTime { get; set; }
        public ICollection<ArticleUser> ArticleUsers { get; set; }
        public ICollection<ArticleTag> ArticleTags { get; set; }
    }
}