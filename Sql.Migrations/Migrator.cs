using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Dapper;
using System.Collections;

namespace Sql.Migrations
{
    public static class Migrator
    {
        public static IEnumerable<string> SplitIntoStatements(string migrationScript)
        {
            var sb = new StringBuilder();
            var allScriptLines = migrationScript.Split(new[] { '\r', '\n' }, StringSplitOptions.None);
            foreach (var line in allScriptLines)
            {
                // Split the script on the 'GO' keyword and execute it sequentially,
                // because SqlCommand can't handle 'GO' itself
                if (line.Trim().Equals("GO", StringComparison.OrdinalIgnoreCase))
                {
                    yield return sb.ToString();
                    sb.Clear();
                }
                else
                {
                    sb.AppendLine(line);
                }
            }

            if (sb.Length > 0)
                yield return sb.ToString();

        }

        public static void ExecuteMigrations(this IDbConnection connection, IEnumerable<MigrationScript> migrationScripts, bool createMigrationsTable = false)
        {
            var migrationScriptsToExecute = migrationScripts;

            using (var transaction = connection.BeginTransaction())
            {
                var emptyDatabase = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.tables", transaction: transaction) == 0;

                if (!emptyDatabase)
                {
                    var migrationsExecuted = connection.Query<string>("SELECT Filename FROM dbo.Migrations", transaction: transaction).ToList();
                    migrationScriptsToExecute = migrationScriptsToExecute
                        .Where(migrationScript => !migrationsExecuted.Contains(migrationScript.Name, StringComparer.OrdinalIgnoreCase));
                }
                else if(createMigrationsTable)
                {
                    CreateMigrationsTable(connection, transaction);
                }

                foreach (var migrationScript in migrationScriptsToExecute)
                {
                    ExecuteMigration(connection, transaction, migrationScript);
                }

                transaction.Commit();
            }
        }
       

        public static void ExecuteMigration(IDbConnection connection, IDbTransaction transaction, MigrationScript migrationScript)
        {
            Console.WriteLine($"Executing migration script {migrationScript.Name}...");

            var statements = SplitIntoStatements(migrationScript.Script).ToList();

            Console.WriteLine($"Found {statements.Count} Statements");

            statements.ForEach(s => connection.Execute(s, transaction: transaction));

            connection.Execute("INSERT INTO [dbo].[Migrations] (Id, Filename, AppliedOn) VALUES (@id, @filename, @appliedOn)", new
            {
                Id = Guid.NewGuid(),
                AppliedOn = DateTimeOffset.Now,
                Filename = migrationScript.Name
            }, transaction);
        }

        // TODO: keep this here or move to file?
        private const string CreateTableScript = @"CREATE TABLE [dbo].[Migrations](
  [Id] [nchar](36) NOT NULL,
  [Filename] [nvarchar](255) NOT NULL,
  [AppliedOn] [datetimeoffset](7) NOT NULL,
  CONSTRAINT PK_Migrations PRIMARY KEY (Id)
)";

        public static void CreateMigrationsTable(IDbConnection connection, IDbTransaction transaction)
        {
            var migrationTableMigration = new MigrationScript
            {
                Name = "Migration-Table",
                Script = CreateTableScript
            };
            ExecuteMigration(connection, transaction, migrationTableMigration);
        }
    }
}
