using System;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Data.SqlClient;
using McMaster.Extensions.CommandLineUtils;
using Microsoft.Data.Sqlite;
using Npgsql;
using Serilog;

namespace PlainSql.Migrations.Tool
{
    class Program
    {
        public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

        [Required]
        [Option(Description = "The connection string", ShortName = "c", LongName = "connectionstring")]
        public string ConnectionString { get; }

        [Option(Description = "The folder where the migration scripts are located", ShortName = "m", LongName = "migrationscriptfolder")]
        public string MigrationScriptFolder { get; set; } = "./MigrationScripts";

        [Required]
        [AllowedValues("mssql", "postgres", "sqllite", IgnoreCase = true)]
        [Option(Description = "The database type to connect to", ShortName = "d", LongName = "databasetype")]
        public string DatabaseType { get; set; }

        [Option(Description = "Should migrations be logged in a migrations table", ShortName = "t", LongName = "createmigrationstable")]
        public bool CreateMigrationsTable { get; set; } = true;

        private void OnExecute()
        {
            //TODO Should we validate the scripts folder path
            Log.Logger = new LoggerConfiguration()
                .WriteTo.Console()
                .CreateLogger();

            Log.Information("Executing database migrations in {MigrationScriptFolder} using {ConnectionString}", MigrationScriptFolder, ConnectionString);
            try
            {
                IDbConnection connection = null;
                switch (DatabaseType)
                {
                    case "mssql":
                        connection = new SqlConnection(ConnectionString);
                        break;
                    case "postgres":
                        connection = new NpgsqlConnection(ConnectionString);
                        break;
                    case "sqllite":
                        connection = new SqliteConnection(ConnectionString);
                        break;
                }

                connection.Open();
                var migrationScripts = MigrationScriptsLoader.FromDirectory(MigrationScriptFolder);
                Migrator.ExecuteMigrations(connection, migrationScripts, CreateMigrationsTable);
                connection.Close();
                Log.Information("Finished executing database migrations!");
            }
            catch (Exception e)
            {
                Log.Error(e, "There has been a problem executing the migrations:");
                throw;
            }
        }
    }
}
