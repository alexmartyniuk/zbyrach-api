using Zbyrach.Api.Account;
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Zbyrach.Api.Mailing
{
    public class MailingSettings : Entity
    {
        public string Schedule { get; set; } = default!;
        public DateTime UpdatedAt { get; set; } = default!;
        public long NumberOfArticles { get; set; } = default!;
        public long UserId { get; set; } = default!;
        public User User { get; set; } = default!;
    }

    public class MailingSettingsConfiguration : IEntityTypeConfiguration<MailingSettings>
    {
        public void Configure(EntityTypeBuilder<MailingSettings> builder)
        {
            builder
                .Property(m => m.Id)
                .IsRequired();
            builder
                .Property(m => m.Schedule)
                .IsRequired();
            builder
                .Property(m => m.NumberOfArticles)
                .IsRequired();
            builder
                .HasOne(m => m.User)
                .WithOne(u => u.MailingSettings);
        }
    }
}