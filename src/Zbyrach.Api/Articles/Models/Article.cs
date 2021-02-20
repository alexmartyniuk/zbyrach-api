using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zbyrach.Api.Articles
{
    public class Article : Entity
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Language { get; set; }
        public DateTime PublicatedAt { get; set; }
        public DateTime FoundAt { get; set; }
        public string Url { get; set; } = default!;
        public string? IllustrationUrl { get; set; }
        public string? AuthorName { get; set; }
        public string? AuthorEmail { get; set; }
        public string? AuthorPhoto { get; set; }
        public long CommentsCount { get; set; }
        public long LikesCount { get; set; }
        public string? ReadTime { get; set; }
        public ICollection<ArticleUser> ArticleUsers { get; }
        public ICollection<ArticleTag> ArticleTags { get; }

        public Article()
        {
            ArticleUsers = new List<ArticleUser>();
            ArticleTags = new List<ArticleTag>();
        }
    }

    public class ArticleConfiguration : IEntityTypeConfiguration<Article>
    {
        public void Configure(EntityTypeBuilder<Article> builder)
        {
            builder
                .Property(a => a.Id)
                .IsRequired();
            builder
                .Property(a => a.Url)
                .IsRequired();
            builder
                .Property(a => a.Title)
                .IsRequired();
            builder
                .Property(a => a.AuthorName)
                .IsRequired();
            builder                
                .HasAlternateKey(a => new { a.Title, a.AuthorName });
        }
    }
}