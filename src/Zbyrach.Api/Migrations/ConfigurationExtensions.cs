using System;
using Microsoft.Extensions.Configuration;

namespace Zbyrach.Api.Migrations
{
    public static class ConfigurationExtensions
    {
        public static string GetConnectionString(this IConfiguration configuration)
        {
            var uri = new UriBuilder(configuration["DATABASE_URL"]);
            return $"Host={uri.Host};" + 
                   $"Database={uri.Path.Remove(0,1)};" + 
                   $"Username={uri.UserName};" + 
                   $"Password={uri.Password};" + 
                    "sslmode=Require;" + 
                    "Trust Server Certificate=true;";
        }
    }
}