using System.Collections.Generic;

namespace PlainSql.Migrations
{
    public class MigrationOptions
    {
        public bool CreateMigrationsTable { get; set; }
        public List<IMigrationScriptProcessor> ScriptProcessors { get; internal set; }
    }
}
