using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Zbyrach.Api.Migrations;

namespace Zbyrach.Api.Tests.Common
{
    public abstract class BaseDatabaseTests : IDisposable
    {
        protected ApplicationContext Context { get; private set; }
        protected SqliteConnection Connection { get; private set; }

        public static readonly LoggerFactory _consoleLogger =
            new LoggerFactory(new[] {
                new Microsoft.Extensions.Logging.Debug.DebugLoggerProvider()
            });
        private readonly DbContextOptions<ApplicationContext> _options;

        public BaseDatabaseTests()
        {
            Connection = new SqliteConnection("DataSource=:memory:");
            Connection.Open();

            _options = new DbContextOptionsBuilder<ApplicationContext>()
                .UseLoggerFactory(_consoleLogger)
                .UseSqlite(Connection)
                .Options;

            SaveAndRecreateContext();
        }

        protected void SaveAndRecreateContext()
        {
            Context?.SaveChanges();
            RecreateContext();
        }

        protected void RecreateContext()
        {
            Context = new ApplicationContext(_options);
            Context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
        }
    }
}