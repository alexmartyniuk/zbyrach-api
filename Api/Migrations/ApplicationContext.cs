using MediumGrabber.Api.Account;
using MediumGrabber.Api.Articles;
using MediumGrabber.Api.Mailing;
using MediumGrabber.Api.Tags;
using MediumGrabber.Api.Readings;
using Microsoft.EntityFrameworkCore;

namespace MediumGrabber.Api.Migrations
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagUser> TagUsers { get; set; }
        public DbSet<MailingSettings> MailingSettings { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<Reading> Readings { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=medium_grabber.db");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
               .Property(p => p.Id)
               .IsRequired();
            modelBuilder.Entity<User>()
               .Property(p => p.Name)
               .IsRequired();
            modelBuilder.Entity<User>()
               .Property(p => p.Email)
               .IsRequired();
            modelBuilder.Entity<User>()
               .HasIndex(p => p.Email)
               .IsUnique();

            modelBuilder.Entity<AccessToken>()
               .Property(p => p.Id)
               .IsRequired();
            modelBuilder.Entity<AccessToken>()
               .Property(p => p.Token)
               .IsRequired();
            modelBuilder.Entity<AccessToken>()
               .HasIndex(p => p.Token)
               .IsUnique();
            modelBuilder.Entity<AccessToken>()
               .Property(p => p.ExpiredAt)
               .IsRequired();
            modelBuilder.Entity<AccessToken>()
               .Property(p => p.UserId)
               .IsRequired();
            modelBuilder.Entity<AccessToken>()
               .HasOne(m => m.User)
               .WithMany(u => u.AccessTokens);

            modelBuilder.Entity<Tag>()
               .Property(p => p.Id)
               .IsRequired();
            modelBuilder.Entity<Tag>()
               .Property(p => p.Name)
               .IsRequired();

            modelBuilder.Entity<TagUser>()
               .HasKey(tu => new { tu.UserId, tu.TagId });
            modelBuilder.Entity<TagUser>()
               .HasOne(tu => tu.User)
               .WithMany(u => u.TagUsers)
               .HasForeignKey(tu => tu.UserId);
            modelBuilder.Entity<TagUser>()
               .HasOne(tu => tu.Tag)
               .WithMany(t => t.TagUsers)
               .HasForeignKey(tu => tu.TagId);

            modelBuilder.Entity<MailingSettings>()
               .Property(m => m.Id)
               .IsRequired();
            modelBuilder.Entity<MailingSettings>()
               .Property(m => m.Schedule)
               .IsRequired();
            modelBuilder.Entity<MailingSettings>()
               .Property(m => m.NumberOfArticles)
               .IsRequired();
            modelBuilder.Entity<MailingSettings>()
               .HasOne(m => m.User)
               .WithOne(u => u.MailingSettings);

            modelBuilder.Entity<Article>()
               .Property(a => a.Id)
               .IsRequired();
            modelBuilder.Entity<Article>()
               .Property(a => a.ExternalId)
               .IsRequired();
            modelBuilder.Entity<Article>()
               .HasIndex(a => a.ExternalId)
               .IsUnique();
            modelBuilder.Entity<Article>()
               .Property(m => m.Url)
               .IsRequired();
            modelBuilder.Entity<Article>()
               .HasOne(a => a.Tag)
               .WithMany(t => t.Articles);

            modelBuilder.Entity<Reading>()
               .Property(r => r.Id)
               .IsRequired();
            modelBuilder.Entity<Reading>()
               .HasIndex(r => new { r.ArticleId, r.UserId });
            modelBuilder.Entity<Reading>()
               .HasOne(r => r.User)
               .WithMany(u => u.Readings)
               .HasForeignKey(r => r.UserId);
            modelBuilder.Entity<Reading>()
               .HasOne(r => r.Article)
               .WithMany(a => a.Readings)
               .HasForeignKey(r => r.ArticleId);
        }
    }
}