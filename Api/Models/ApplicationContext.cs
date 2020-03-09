using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<AccessToken> AccessTokens { get; set; }

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
                .Property(p => p.Provider)
                .IsRequired();
            modelBuilder.Entity<AccessToken>()
                .Property(p => p.ExpiredAt)
                .IsRequired();
            modelBuilder.Entity<AccessToken>()
                .Property(p => p.UserId)
                .IsRequired();
            modelBuilder.Entity<AccessToken>()
                .HasOne(m => m.User)
                .WithMany(u => u.AccessTokens);
        }
    }
}