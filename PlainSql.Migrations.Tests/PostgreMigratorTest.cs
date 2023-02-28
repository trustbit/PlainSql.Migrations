using System;
using System.Data;
using System.Threading.Tasks;
using Npgsql;
using Xunit;

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
                return @"Server=127.0.0.1;User Id=postgres;Password=Password12!";
            }

            return "Server=127.0.0.1;User Id=postgres;Password=postgres";
        }

        [Fact]
        public Task Can_execute_scripts_in_parallel()
        {
            return ExecuteMigrationsInParallel(() => Connection);
        }
    }
}
