using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

using Dapper;
using System.Collections;
using Serilog;

namespace PlainSql.Migrations
{
    public static class Migrator
    {
        // TODO: Add a script hash to migrations to avoid scripts to be changed without running the migration
        public static void ExecuteMigrations(this IDbConnection connection, IEnumerable<MigrationScript> migrationScripts, bool createMigrationsTable = true)
        {
            using (var transaction = connection.BeginTransaction())
            {
                 var containsMigrationTable = connection.ExecuteScalar<int>("SELECT COUNT(*) FROM INFORMATION_SCHEMA.tables WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Migrations'",
                        transaction: transaction) == 1;

                if (createMigrationsTable && !containsMigrationTable)
                {
                    Log.Information("Creating initial migration table...");
                    CreateMigrationsTable(connection, transaction);
                }

                var migrationsExecuted = connection.Query<string>("SELECT Filename FROM dbo.Migrations", transaction: transaction).ToList();

                var migrationScriptsToExecute = migrationScripts
                    .Where(migrationScript => !migrationsExecuted.Contains(migrationScript.Name, StringComparer.OrdinalIgnoreCase))
                    .ToList();


                Log.Information("Executing {MigrationScriptsCount} migration scripts...", migrationScriptsToExecute.Count());

                foreach (var migrationScript in migrationScriptsToExecute)
                {
                    ExecuteMigration(connection, transaction, migrationScript);
                }

                transaction.Commit();
            }
        }

        public static void ExecuteMigration(IDbConnection connection, IDbTransaction transaction, MigrationScript migrationScript)
        {
            var statements = SplitIntoStatements(migrationScript.Script).ToList();

            Log.Information("Executing {StatementsCount} statements in migration script {MigrationScriptName}...", statements.Count, migrationScript.Name);

            statements.ForEach(s => connection.Execute(s, transaction: transaction));

            connection.Execute("INSERT INTO [dbo].[Migrations] (Id, Filename, AppliedOn) VALUES (@id, @filename, @appliedOn)", new
            {
                Id = Guid.NewGuid(),
                AppliedOn = DateTimeOffset.Now,
                Filename = migrationScript.Name
            }, transaction);
        }

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
    }
}
