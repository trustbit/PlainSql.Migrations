using System;
using System.Data;
using System.Data.SqlClient;
using System.Threading.Tasks;
using Xunit;

namespace PlainSql.Migrations.Tests
{
    public class MsSqlMigratorTests : AbstractMigratorTests
    {
        protected override IDbConnection Connection
        {
            get
            {
                var connectionString = GetConnectionString();

                var c = new SqlConnection(connectionString);
                c.Open();

                return c;
            }
        }

        protected string GetConnectionString()
        {
            var connectionStringFromEnvironment = Environment.GetEnvironmentVariable("PLAIN_SQL_MIGRATIONS_MS_SQL");

            if (!String.IsNullOrWhiteSpace(connectionStringFromEnvironment))
            {
                return connectionStringFromEnvironment;
            }

            var IsAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

            if (IsAppVeyor)
            {
                return @"Server=(local)\SQL2016;Database=tempdb;User ID=sa;Password=Password12!";
            }

            return "Data Source=localhost;Initial Catalog=PlainSqlMigrations;User id=SA;Password=test123!;";
        }

        [Fact]
        public Task Can_execute_scripts_in_parallel()
        {
            return ExecuteMigrationsInParallel(() => Connection);
        }
    }
}
