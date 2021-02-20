using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zbyrach.Api.Account;

namespace Zbyrach.Api.Articles
{
    public class ArticleUser : Entity
    {
        public long UserId { get; set; }
        public User User { get; set; } = default!;
        public long ArticleId { get; set; }
        public Article Article { get; set; } = default!;
        public ArticleStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime SentAt { get; set; }
        public DateTime ReadAt { get; set; }
        public bool Favorite { get; set; }
        public bool ReadLater { get; set; }
    }

    public class ArticleUserConfiguration : IEntityTypeConfiguration<ArticleUser>
    {
        public void Configure(EntityTypeBuilder<ArticleUser> builder)
        {
            builder
                .Property(r => r.Id)
                .IsRequired();
            builder
                .HasIndex(r => new { r.ArticleId, r.UserId });
            builder
                 .HasOne(r => r.User)
                .WithMany(u => u.ArticleUsers)
                .HasForeignKey(r => r.UserId);
            builder
                .HasOne(r => r.Article)
                .WithMany(a => a.ArticleUsers)
                .HasForeignKey(r => r.ArticleId);
        }
    }
}