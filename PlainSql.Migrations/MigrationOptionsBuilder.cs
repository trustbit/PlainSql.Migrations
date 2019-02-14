using System.Collections.Generic;

namespace PlainSql.Migrations
{
    public class MigrationOptionsBuilder
    {
        private bool _createMigrationsTable;

        private List<IMigrationScriptProcessor> _scriptProcessors = new List<IMigrationScriptProcessor>();

        public MigrationOptionsBuilder CreateMigrationsTable(bool createMigrationsTable) {
            _createMigrationsTable = createMigrationsTable;
            return this;
        }

        public MigrationOptionsBuilder Use(IMigrationScriptProcessor scriptProcessor)
        {
            _scriptProcessors.Add(scriptProcessor);

            return this;
        }

        public MigrationOptions Build()
        {
            return new MigrationOptions
            {
                CreateMigrationsTable = _createMigrationsTable,
                ScriptProcessors = _scriptProcessors
            };
        }
    }
}
