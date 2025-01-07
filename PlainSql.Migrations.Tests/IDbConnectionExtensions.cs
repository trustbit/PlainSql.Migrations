using Dapper;
using System;
using System.Data;

namespace PlainSql.Migrations.Tests
{
    public static class IDbConnectionExtensions
    {
        public static void WithCleanDbConnection(this IDbConnection connection, Action<IDbConnection> action)
        {
            using (var c = connection)
            {
                using (var tx = c.BeginTransaction())
                {
                    c.Execute("DROP TABLE IF EXISTS bla", transaction: tx);
                    c.Execute("DROP TABLE IF EXISTS Migrations", transaction: tx);

                    tx.Commit();
                }

                action(c);
            }
        }
    }
}
