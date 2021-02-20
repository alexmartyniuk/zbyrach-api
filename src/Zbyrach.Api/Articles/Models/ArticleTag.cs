using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zbyrach.Api.Tags;

namespace Zbyrach.Api.Articles
{
    public class ArticleTag
    {
        public long TagId { get; set; }
        public Tag Tag { get; set; } = default!;
        public long ArticleId { get; set; }
        public Article Article { get; set; } = default!;
    }

    public class ArticleTagConfiguration : IEntityTypeConfiguration<ArticleTag>
    {
        public void Configure(EntityTypeBuilder<ArticleTag> builder)
        {
            builder
                .HasKey(at => new { at.ArticleId, at.TagId });
            builder
                .HasOne(at => at.Article)
                .WithMany(a => a.ArticleTags)
                .HasForeignKey(at => at.ArticleId);
            builder
                .HasOne(at => at.Tag)
                .WithMany(t => t.ArticleTags)
                .HasForeignKey(at => at.TagId);
        }
    }
}