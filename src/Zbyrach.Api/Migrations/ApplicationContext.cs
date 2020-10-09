using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing;
using Zbyrach.Api.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace Zbyrach.Api.Migrations
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<TagUser> TagUsers { get; set; }
        public DbSet<MailingSettings> MailingSettings { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<ArticleUser> ArticleUsers { get; set; }
        public DbSet<ArticleTag> ArticleTags { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options) : base(options)
        {
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            if (!optionsBuilder.IsConfigured)
            {
                var config = new ConfigurationBuilder()
                    .SetBasePath(Directory.GetCurrentDirectory())
                    .AddJsonFile("appsettings.json", false, true)
                    .AddJsonFile("appsettings.Development.json", true)
                    .AddEnvironmentVariables()
                    .Build();

                optionsBuilder.UseNpgsql(config.GetConnectionString());
            }
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
               .Property(p => p.CreatedAt)
               .IsRequired();
            modelBuilder.Entity<AccessToken>()
               .Property(p => p.UserId)
               .IsRequired();
            modelBuilder.Entity<AccessToken>()
               .HasOne(m => m.User)
               .WithMany(u => u.AccessTokens);
            modelBuilder.Entity<AccessToken>()
               .HasIndex(p => p.ClientIp);
            modelBuilder.Entity<AccessToken>()
               .HasIndex(p => p.ClientUserAgent);

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
               .Property(m => m.Url)
               .IsRequired();            

            modelBuilder.Entity<ArticleUser>()
               .Property(r => r.Id)
               .IsRequired();
            modelBuilder.Entity<ArticleUser>()
               .HasIndex(r => new { r.ArticleId, r.UserId });
            modelBuilder.Entity<ArticleUser>()
               .HasOne(r => r.User)
               .WithMany(u => u.ArticleUsers)
               .HasForeignKey(r => r.UserId);
            modelBuilder.Entity<ArticleUser>()
               .HasOne(r => r.Article)
               .WithMany(a => a.ArticleUsers)
               .HasForeignKey(r => r.ArticleId);

            modelBuilder.Entity<ArticleTag>()
               .HasKey(at => new { at.ArticleId, at.TagId });
            modelBuilder.Entity<ArticleTag>()
               .HasOne(at => at.Article)
               .WithMany(a => a.ArticleTags)
               .HasForeignKey(at => at.ArticleId);
            modelBuilder.Entity<ArticleTag>()
               .HasOne(at => at.Tag)
               .WithMany(t => t.ArticleTags)
               .HasForeignKey(at => at.TagId);
        }
    }
}