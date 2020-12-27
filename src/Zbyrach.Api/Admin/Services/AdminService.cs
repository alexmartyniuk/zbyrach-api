using System.Threading.Tasks;
using Dapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Npgsql;
using Zbyrach.Api.Migrations;

namespace Zbyrach.Api.Admin.Services
{
    public class AdminService
    {
        private readonly ApplicationContext _db;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminService> _logger;

        public AdminService(ApplicationContext db, IConfiguration configuration, ILogger<AdminService> logger)
        {
            _db = db;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<int> GetTotalRowsCount()
        {
            var sql = $"SELECT SUM(n_live_tup) FROM pg_stat_user_tables;";
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString());
            return await connection.QuerySingleAsync<int>(sql);
        }

        public async Task<long> GetTotalSizeInBytes()
        {
            var databaseName = _configuration.GetDatabaseName();
            var sql = $"SELECT pg_database_size('{databaseName}');";
            await using var connection = new NpgsqlConnection(_configuration.GetConnectionString());
            return await connection.QuerySingleAsync<int>(sql);
        }

        public Task<int> GetArticlesCount()
        {
            return _db.Articles.CountAsync();
        }

        public Task<int> GetUsersCount()
        {
            return _db.Users.CountAsync();
        }

        public Task<int> GetTagsCount()
        {
            return _db.Tags.CountAsync();
        }

        public async Task Cleanup(int daysCleanup)
        {
            _logger.LogInformation($"We are goint to remove articles older than {daysCleanup} days.");
            var affectedRows = await _db.Database
                .ExecuteSqlRawAsync($@"DELETE FROM ""Articles"" WHERE ""FoundAt"" < now() - INTERVAL '{daysCleanup} days'");

            _logger.LogInformation($"Articles that are older that {daysCleanup} days were deleted from database: {affectedRows}.");
        }
    }
}