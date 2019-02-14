using System;
using System.Data;
using System.Text.RegularExpressions;

namespace PlainSql.Migrations.Sqlite
{
    public class FixSqliteMaxProcessor : IMigrationScriptProcessor
    {
        private static readonly Regex FixMax = new Regex("\\(MAX\\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        private readonly int _valueOfMax;

        public FixSqliteMaxProcessor(int valueOfMax)
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
                Script = FixMax.Replace(script.Script, $"({_valueOfMax.ToString()})"),
            };
        }
    }
}
