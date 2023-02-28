using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Data.SQLite;
using System.Linq;
using System.Threading.Tasks;

namespace PlainSql.Migrations.Tests
{
    public static class IDbConnectionExtensions
    {
        public static void CleanDbConnection(this IDbConnection connection)
        {
            using (var tx = connection.BeginTransaction())
            {
                connection.Execute("DROP TABLE IF EXISTS bla", transaction: tx);
                connection.Execute("DROP TABLE IF EXISTS Migrations", transaction: tx);

                tx.Commit();
            }
        }

        public static void WithCleanDbConnection(this IDbConnection connection, Action<IDbConnection> action)
        {
            using (var c = connection)
            {
                c.CleanDbConnection();

                action(c);
            }
        }
    }
}
