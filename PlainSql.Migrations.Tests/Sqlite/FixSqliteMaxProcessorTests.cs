
using PlainSql.Migrations.Sqlite;
using Xunit;

namespace PlainSql.Migrations.Tests.Sqlite
{
    public class FixSqliteMaxProcessorTests
    {
        [Fact]
        public void ShouldReplaceMax()
        {
            var processor = new FixSqliteMaxProcessor(8000);
            var migrationScript = new MigrationScript {
                Name = "test.sql",
                Script = @"CREATE TABLE TestTable
(
    Id UNIQUEIDENTIFIER PRIMARY KEY,

    Title NVARCHAR(256) NOT NULL,
    Content NVARCHAR(MAX) NULL
)"
            };

            var processedScript = processor.Process(null, migrationScript);

            var expectedScript = @"CREATE TABLE TestTable
(
    Id UNIQUEIDENTIFIER PRIMARY KEY,

    Title NVARCHAR(256) NOT NULL,
    Content NVARCHAR(8000) NULL
)";

            Assert.Equal(expectedScript, processedScript.Script);
            Assert.Equal(migrationScript.Name, processedScript.Name);
        }

        [Fact]
        public void ShouldNotReplaceMaxWithoutBraces()
        {
            var processor = new FixSqliteMaxProcessor(8000);
            var migrationScript = new MigrationScript {
                Name = "test.sql",
                Script = @"CREATE TABLE TestTable
(
    Id UNIQUEIDENTIFIER PRIMARY KEY,

    Title NVARCHAR(256) NOT NULL,
    Max NVARCHAR(256) NULL
)"
            };

            var expectedScript = migrationScript.Script;
            var processedScript = processor.Process(null, migrationScript);

            Assert.Equal(expectedScript, processedScript.Script);
            Assert.Equal(migrationScript.Name, processedScript.Name);
        }
    }
}
