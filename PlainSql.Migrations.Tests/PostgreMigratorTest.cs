using System;
using System.Data;
using Npgsql;

namespace PlainSql.Migrations.Tests
{
    public class PostgreMigratorTest : AbstractMigratorTests
    {
        protected override IDbConnection Connection
        {
            get
            {
                var connectionString = GetConnectionString();

                var c = new NpgsqlConnection(connectionString);
                c.Open();

                return c;
            }
        }

        protected string GetConnectionString()
        {
            var connectionStringFromEnvironment = Environment.GetEnvironmentVariable("PLAIN_SQL_MIGRATIONS_POSTGRE_SQL");

            if (!String.IsNullOrWhiteSpace(connectionStringFromEnvironment))
            {
                return connectionStringFromEnvironment;
            }

            var IsAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

            if (IsAppVeyor)
            {
                return "Server=127.0.0.1;User Id=postgres;Password=Password12!";
            }

            var IsGitHubActions = Environment.GetEnvironmentVariable("CI")?.ToUpperInvariant() == "true";

            if (IsGitHubActions)
            {
                return "Server=postgres;Password=postgres";
            }

            return "Server=127.0.0.1;User Id=postgres;Password=postgres";
        }
    }
}
