using System.Data;

namespace PlainSql.Migrations
{
    public interface IMigrationScriptProcessor
    {
        bool CanProcess(IDbConnection connection, MigrationScript script);

        MigrationScript Process(IDbConnection connection, MigrationScript script);
    }
}
