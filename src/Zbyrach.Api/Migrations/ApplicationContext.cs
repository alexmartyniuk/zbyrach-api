using Zbyrach.Api.Account;
using Zbyrach.Api.Articles;
using Zbyrach.Api.Mailing;
using Zbyrach.Api.Tags;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Reflection;

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

        public ApplicationContext()
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
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
    }
}