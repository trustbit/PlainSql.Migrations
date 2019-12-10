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
        public static void WithCleanDbConnection(this IDbConnection connection, Action<IDbConnection> action)
        {
            using (var c = connection)
            {
                using (var tx = c.BeginTransaction())
                {
                    c.Execute(c.IsPostgre() ? "DROP TABLE IF EXISTS bla" : "DROP TABLE IF EXISTS [bla]", transaction: tx);
                    c.Execute(c.IsPostgre() ? "DROP TABLE IF EXISTS Migrations" : "DROP TABLE IF EXISTS [Migrations]", transaction: tx);

                    tx.Commit();
                }

                action(c);
            }
        }
    }
}
