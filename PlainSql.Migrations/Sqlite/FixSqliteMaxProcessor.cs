using System;
using System.Data;

namespace PlainSql.Migrations.Sqlite
{
    public class FixSqliteMax : IMigrationScriptProcessor
    {
        private readonly int _valueOfMax;

        public FixSqliteMax(int valueOfMax)
        {
            _valueOfMax = valueOfMax;
        }

        public bool CanProcess(IDbConnection connection, MigrationScript script)
        {
            return connection.IsSqlite();
        }

        public MigrationScript Process(IDbConnection connection, MigrationScript script)
        {
            return new MigrationScript
            {
                Name = script.Name,
                Script = script.Script.Replace("(MAX)", $"({_valueOfMax.ToString()})"),
            };
        }
    }
}
