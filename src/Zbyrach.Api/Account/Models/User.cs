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
        public string Name { get; set; }
        public string Email { get; set; }
        public string PictureUrl { get; set; }
        public bool IsAdmin { get; set; }
        public string Language { get; set; }
        public ICollection<AccessToken> AccessTokens { get; set; }
        public ICollection<TagUser> TagUsers { get; set; }
        public MailingSettings MailingSettings { get; set; }
        public ICollection<ArticleUser> ArticleUsers { get; set; }
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