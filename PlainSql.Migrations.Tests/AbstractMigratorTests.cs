using Dapper;
using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace PlainSql.Migrations.Tests
{
    public abstract class AbstractMigratorTests
    {
        protected abstract IDbConnection Connection { get; }

        [Fact]
        public void String_with_no_GO_is_only_one_script()
        {
            var test = @"CREATE TABLE [Migrations](
        [Id] [uniqueidentifier] NOT NULL,
        [Filename] [nvarchar](255) NOT NULL,
        [AppliedOn] [datetimeoffset](7) NOT NULL,
 CONSTRAINT [PK_Migrations] PRIMARY KEY CLUSTERED
(
        [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY]";

            Assert.Single(Migrator.SplitIntoStatements(test));
        }

        [Fact]
        public void String_with_two_GOs_should_result_in_two_Statements()
        {
            var twoGos = @"/****** CREATE TABLES HERE ******/

/*

CREATE TABLE ....

Sripts can be generated with SQL Server Management Studio,
or with a Visual Studio database project

*/

CREATE TABLE [Foo](
        [Id] [uniqueidentifier] NOT NULL,

PRIMARY KEY CLUSTERED
(
        [Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

ALTER TABLE [Bar] CHECK CONSTRAINT [FooBar]
GO";

            // TODO: Discuss: should we remove empty lines?
            Assert.Collection(Migrator.SplitIntoStatements(twoGos),
                x => Assert.StartsWith("/****", x),
                x => Assert.StartsWith("ALTER", RemoveEmptyLines(x))
                );
        }

        [Fact]
        public void On_empty_db_can_execute_no_Scripts()
        {
            Connection.WithCleanDbConnection((c) =>
            {
                Migrator.ExecuteMigrations(c, Enumerable.Empty<MigrationScript>());
            });
        }

        [Fact]
        public void On_empty_db_can_create_a_Migrations_table()
        {
            Connection.WithCleanDbConnection((c) =>
            {
                using (var tx = c.BeginTransaction())
                {
                    Migrator.CreateMigrationsTable(c, tx);
                    tx.Commit();
                }

                var select = c.IsPostgre() ? "SELECT Filename FROM Migrations" : "SELECT Filename FROM [Migrations]"; 

                Assert.NotEmpty(c.Query<string>(select));
            });
        }

        [Fact]
        public void Can_execute_one_create_Table_Script_in_empty_Db()
        {
            Connection.WithCleanDbConnection((c) =>
            {
                var defaultScript = "CREATE TABLE [bla]([Id] [uniqueidentifier] NOT NULL)";
                var postgreScript = "CREATE TABLE bla (Id varchar(1) NOT NULL)";
                var migration = new MigrationScript
                {
                    Name = "create bla table",
                    Script = c.IsPostgre() ? postgreScript : defaultScript
                };
                Migrator.ExecuteMigrations(c, new[] { migration }, true);

                var select = c.IsPostgre() ? "SELECT * FROM bla" : "SELECT * FROM [bla]"; 

                Assert.Empty(c.Query<object>(select));
            });
        }

        [Fact]
        public void Does_not_execute_the_same_script_twice()
        {
            Connection.WithCleanDbConnection((c) =>
            {
                var defaultScript = "CREATE TABLE [bla]([Id] [uniqueidentifier] NOT NULL)";
                var postgreScript = "CREATE TABLE bla (Id varchar(1) NOT NULL)";
                var migration = new MigrationScript
                {
                    Name = "create bla table",
                    Script = c.IsPostgre() ? postgreScript : defaultScript
                };
                Migrator.ExecuteMigrations(c, new[] { migration }, true);

                Migrator.ExecuteMigrations(c, new[] { migration }, true);
                
                var select = c.IsPostgre() ? "SELECT Filename FROM Migrations" : "SELECT Filename FROM [Migrations]"; 
                // Migrations table + bla
                Assert.Equal(2, c.Query<string>(select).ToList().Count);
            });
        }

        private string RemoveEmptyLines(string x)
        {
            return String.Join("\r\n",
                x.Split(new[] { "\r\n", "\r", "\n" }, StringSplitOptions.None)
                .Where(s => !String.IsNullOrWhiteSpace(s)));
        }
    }
}
