using System;
using System.Collections.Generic;

namespace Zbyrach.Api.Mailing.Templates
{
    public class ArticlesModel
    {
        public string UserName { get; set; } = default!;
        public string UserEmail { get; set; } = default!;
        public string Period { get; set; } = default!;
        public string ViewOnSiteUrl { get; set; } = default!;
        public string UnsubscribeUrl { get; set; } = default!;
        public List<ArticleModel> Articles { get; set; } = default!;
    }

    public class ArticleModel
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? PublicatedAt { get; set; }
        public string Url { get; set; } = default!;
        public string PdfUrl { get; set; } = default!;
        public string AuthorName { get; set; } = default!;
        public string AuthorEmail { get; set; } = default!;
        public string AuthorPhoto { get; set; } = default!;
        public long CommentsCount { get; set; } = default!;
        public long LikesCount { get; set; } = default!;
        public string? ReadTime { get; set; } = default!;
    }
}