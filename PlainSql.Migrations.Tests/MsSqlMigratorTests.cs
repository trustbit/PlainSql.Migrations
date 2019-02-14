using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
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
            var connectinStringFromEnvironment = Environment.GetEnvironmentVariable("PLAIN_SQL_MIGRATIONS_MS_SQL");

            if (!String.IsNullOrWhiteSpace(connectinStringFromEnvironment))
            {
                return connectinStringFromEnvironment;
            }

            var IsAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

            if (IsAppVeyor)
            {
                return @"Server=(local)\SQL2016;Database=tempdb;User ID=sa;Password=Password12!";
            }

            return "Data Source=localhost;Initial Catalog=PlainSqlMigrations;User id=SA;Password=test123!;";
        }
    }
}
