using Dapper;
using PlainSql.Migrations.Sqlite;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;
using Xunit;

namespace PlainSql.Migrations.Tests
{
    public class SqliteMigratorTests : AbstractMigratorTests
    {
        protected override IDbConnection Connection
        {
            get
            {
                var c = new SQLiteConnection("Data Source=:memory:;");
                c.Open();

                return c;
            }
        }

        [Fact]
        public void Should_fix_sql_max_with_migration_script_processor()
        {
            var migrationScripts = new List<MigrationScript>
            {
                new MigrationScript {
                    Name = "test.sql",
                    Script = @"CREATE TABLE TestTable
(
    Id UNIQUEIDENTIFIER PRIMARY KEY,

    Title NVARCHAR(256) NOT NULL,
    Content NVARCHAR(MAX) NULL
)"
                }
            };

            Connection.WithCleanDbConnection(connection =>
            {
                Migrator.ExecuteMigrations(connection, migrationScripts, (configuration) =>
                {
                    configuration.Use(new FixSqliteMax(8000));
                });

                Assert.Empty(connection.Query<object>("SELECT * FROM TestTable"));
            });
        }

    }
}
