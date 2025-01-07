using System;
using System.Data;
using System.Data.SqlClient;

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
                return @"Server=(local)\SQL2019;Database=tempdb;User ID=sa;Password=Password12!";
            }

            var IsGitHubActions = Environment.GetEnvironmentVariable("GITHUB_ACTIONS")?.ToUpperInvariant() == "TRUE";

            if (IsGitHubActions)
            {
                return "Data Source=localhost;User id=SA;Password=test123!;";
            }

            return "Data Source=localhost;Initial Catalog=PlainSqlMigrations;User id=SA;Password=test123!;";
        }
    }
}
