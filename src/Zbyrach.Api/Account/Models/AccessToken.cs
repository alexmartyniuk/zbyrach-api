using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zbyrach.Api.Account
{
    public class AccessToken : Entity
    {
        public string? ClientIp { get; set; }
        public string? ClientUserAgent { get; set; }
        public string Token { get; set; } = default!;
        public DateTime CreatedAt { get; set; }
        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime ExpiredAt()
        {
            return CreatedAt + TimeSpan.FromDays(30);
        }
    }

    public class AccessTokenConfiguration : IEntityTypeConfiguration<AccessToken>
    {
        public void Configure(EntityTypeBuilder<AccessToken> builder)
        {
            builder
                .Property(p => p.Id)
                .IsRequired();
            builder
                .Property(p => p.Token)
                .IsRequired();
            builder
                .HasIndex(p => p.Token)
                .IsUnique();
            builder
                .Property(p => p.CreatedAt)
                .IsRequired();
            builder
                .Property(p => p.UserId)
                .IsRequired();
            builder
                 .HasOne(m => m.User)
                .WithMany(u => u.AccessTokens);
            builder
                 .HasIndex(p => p.ClientIp);
            builder
                .HasIndex(p => p.ClientUserAgent);
        }
    }
}