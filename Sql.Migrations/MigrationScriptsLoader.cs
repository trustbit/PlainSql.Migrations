using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Sql.Migrations
{
    public static class MigrationScriptsLoader
    {
        public static IEnumerable<MigrationScript> FromDirectory(string migrationsDirectory, string searchPattern = null)
        {
            var migrationScriptNames = Directory.GetFiles(migrationsDirectory, searchPattern ?? "*.sql");

            return migrationScriptNames
                .Select(filename => new MigrationScript
                {
                    Name = Path.GetFileName(filename),
                    Script = File.ReadAllText(filename)
                })
                .ToList();
        }
    }
}