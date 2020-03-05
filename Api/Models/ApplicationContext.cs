using Microsoft.EntityFrameworkCore;

namespace Api.Models
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users {get; set;}

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            options.UseSqlite("Data Source=medium_grabber.db");
        }        
    }
}