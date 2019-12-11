using System;
using System.Data;

namespace PlainSql.Migrations
{
    public static class DbConnectionExtensions
    {
        public static bool IsSqlite(this IDbConnection connection)
        {
            return connection.GetType().AssemblyQualifiedName.IndexOf("SQLite", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }

        public static bool IsPostgre(this IDbConnection connection)
        {
            return connection.GetType().AssemblyQualifiedName.IndexOf("Npgsql", StringComparison.InvariantCultureIgnoreCase) >= 0;
        }
    }
}
