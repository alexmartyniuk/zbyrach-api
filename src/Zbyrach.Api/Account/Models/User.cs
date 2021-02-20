using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing;
using Zbyrach.Api.Tags;

namespace Zbyrach.Api.Account
{
    public class User : Entity
    {
        public string Name { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? PictureUrl { get; set; }
        public bool IsAdmin { get; set; } = default!;
        public string? Language { get; set; }
        public ICollection<AccessToken> AccessTokens { get; set; } = default!;
        public ICollection<TagUser> TagUsers { get; set; } = default!;
        public MailingSettings MailingSettings { get; set; } = default!;
        public ICollection<ArticleUser> ArticleUsers { get; set; } = default!;
    }

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder
                .Property(p => p.Id)
                .IsRequired();
            builder
                .Property(p => p.Name)
                .IsRequired();
            builder
                .Property(p => p.Email)
                .IsRequired();
            builder
                .HasIndex(p => p.Email)
                .IsUnique();
        }
    }
}