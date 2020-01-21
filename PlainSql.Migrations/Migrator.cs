using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Dapper;
using System.Collections;
using System.Data.SqlClient;
using Serilog;

namespace PlainSql.Migrations
{
    public static class Migrator
    {
        // TODO: Add a script hash to migrations to avoid scripts to be changed without running the migration
        public static void ExecuteMigrations(this IDbConnection connection, IEnumerable<MigrationScript> migrationScripts, bool createMigrationsTable = true)
        {
            ExecuteMigrations(connection, migrationScripts, options => { });
        }

        public static void ExecuteMigrations(this IDbConnection connection, IEnumerable<MigrationScript> migrationScripts, Action<MigrationOptionsBuilder> configure)
        {
            var builder = new MigrationOptionsBuilder().CreateMigrationsTable(true);

            configure(builder);

            var options = builder.Build();

            ExecuteMigrations(connection, migrationScripts, options);
        }

        public static void ExecuteMigrations(this IDbConnection connection, IEnumerable<MigrationScript> migrationScripts, MigrationOptions options)
        {
            var retryCount = 3;
            while (retryCount > 0)
            {
                try
                {
                    TryExecute(connection, migrationScripts, options);
                    return;
                }
                // a sql exception that is a deadlock
                catch (SqlException e) when (e.Number == 1205 && retryCount != 0)
                {
                    Log.Information(e,"{RetryCount} remaining retries for the execution of migrations", retryCount);
                    retryCount--;
                }
            }
        }

        private static void TryExecute(IDbConnection connection, IEnumerable<MigrationScript> migrationScripts, MigrationOptions options)
        {
            using (var transaction = connection.BeginTransaction())
            {
                var containsMigrationTable = false;

                var selectMigrationsExecuted = "SELECT Filename FROM Migrations";

                if (connection.IsSqlite())
                {
                    containsMigrationTable = connection.ExecuteScalar<int>(
                                                 "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='Migrations'",
                                                 transaction: transaction) == 1;
                }
                else if (connection.IsPostgre())
                {
                    connection.Execute("SELECT pg_advisory_xact_lock(1)", transaction: transaction);
                    containsMigrationTable = connection.ExecuteScalar<int>(
                                                 "SELECT COUNT(*) FROM pg_catalog.pg_tables WHERE schemaname='public' AND tablename='migrations'",
                                                 transaction: transaction) == 1;
                }
                else
                {
                    containsMigrationTable = connection.ExecuteScalar<int>(
                                                 "SELECT COUNT(*) FROM INFORMATION_SCHEMA.tables with (SERIALIZABLE) WHERE TABLE_SCHEMA='dbo' AND TABLE_NAME='Migrations' ",
                                                 transaction: transaction) == 1;
                    selectMigrationsExecuted = "SELECT Filename FROM Migrations with(SERIALIZABLE)";
                }

                if (options.CreateMigrationsTable && !containsMigrationTable)
                {
                    Log.Information("Creating initial migration table...");
                    CreateMigrationsTable(connection, transaction);
                }

                var migrationsExecuted = connection.Query<string>(selectMigrationsExecuted, transaction: transaction).ToList();

                var migrationScriptsToExecute = migrationScripts
                    .Where(migrationScript => !migrationsExecuted.Contains(migrationScript.Name, StringComparer.OrdinalIgnoreCase))
                    .ToList();

                Log.Information("Executing {MigrationScriptsCount} migration scripts...", migrationScriptsToExecute.Count());

                foreach (var migrationScript in migrationScriptsToExecute.Select(ProcessMigrationScript(connection, options.ScriptProcessors)))
                {
                    ExecuteMigration(connection, transaction, migrationScript);
                }

                transaction.Commit();
            }
        }

        private static Func<MigrationScript, MigrationScript> ProcessMigrationScript(IDbConnection connection, IEnumerable<IMigrationScriptProcessor> processors)
        {
            return (migrationScript) =>
            {
                var processedScript = migrationScript;

                foreach (var processor in processors)
                {
                    if (processor.CanProcess(connection, processedScript))
                    {
                        processedScript = processor.Process(connection, processedScript);
                    }
                }

                return processedScript;
            };
        }

        public static void ExecuteMigration(IDbConnection connection, IDbTransaction transaction, MigrationScript migrationScript)
        {
            var statements = SplitIntoStatements(migrationScript.Script).ToList();

            Log.Information("Executing {StatementsCount} statements in migration script {MigrationScriptName}...", statements.Count, migrationScript.Name);

            statements.ForEach(s => connection.Execute(s, transaction: transaction));

            connection.Execute("INSERT INTO Migrations (Id, Filename, AppliedOn) VALUES (@id, @filename, @appliedOn)", new
            {
                Id = Guid.NewGuid(),
                AppliedOn = DateTimeOffset.Now,
                Filename = migrationScript.Name
            }, transaction);
        }

        private const string CreateTableScript = @"CREATE TABLE Migrations (
  [Id] [nchar](36) NOT NULL,
  [Filename] [nvarchar](255) NOT NULL,
  [AppliedOn] [datetimeoffset](7) NOT NULL,
  CONSTRAINT PK_Migrations PRIMARY KEY (Id)
)";

        private const string CreateTableScriptPostgre = @"CREATE TABLE Migrations (
  Id char(36) NOT NULL,
  Filename varchar(255) NOT NULL,
  AppliedOn timestamp NOT NULL,
  CONSTRAINT PK_Migrations PRIMARY KEY (Id)
)";

        public static void CreateMigrationsTable(IDbConnection connection, IDbTransaction transaction)
        {
            var migrationTableMigration = new MigrationScript
            {
                Name = "Migration-Table",
                Script = connection.IsPostgre() ? CreateTableScriptPostgre : CreateTableScript
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
