using System;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Zbyrach.Api.Migrations;

namespace Zbyrach.Api.Tests
{
    public abstract class DatabaseTests : IDisposable
    {
        protected ApplicationContext Context { get; private set; }
        protected SqliteConnection Connection { get; private set; }

        public DatabaseTests()
        {
            Connection = new SqliteConnection("DataSource=:memory:");
            Connection.Open();
            SaveAndRecreateContext();
        }

        protected void SaveAndRecreateContext(bool saveChanges = true)
        {
            try
            {
                if (saveChanges && Context != null)
                {
                    Context.SaveChanges();
                }
            }
            finally
            {
                var options = new DbContextOptionsBuilder<ApplicationContext>()
                    .UseSqlite(Connection)
                    .Options;

                Context = new ApplicationContext(options);
                Context.Database.EnsureCreated();
            }
        }

        public void Dispose()
        {
            Connection.Close();
            Connection.Dispose();
        }
    }
}