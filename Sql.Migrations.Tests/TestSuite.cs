using Dapper;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace Sql.Migrations.Tests
{
    public class TestSuite
    {
        private static readonly bool IsAppVeyor = Environment.GetEnvironmentVariable("Appveyor")?.ToUpperInvariant() == "TRUE";

        public static string ConnectionString =>  IsAppVeyor
            ? @"Server=(local)\SQL2014;Database=tempdb;User ID=sa;Password=Password12!"
            : "Data Source=(localdb)\\ProjectsV13;Initial Catalog=tempdb;Integrated Security=True";
        
        public static IDbConnection GetConnection()
        {
            var c = new SqlConnection(ConnectionString);
            c.Open();

            return c;
        }

        public static void WithCleanDbConnection(Action<IDbConnection> action)
        {
            using (var c = GetConnection())
            {
                using (var tx = c.BeginTransaction())
                {
                    c.Execute("DROP TABLE IF EXISTS [dbo].[bla]", transaction: tx);
                    c.Execute("DROP TABLE IF EXISTS [dbo].[Migrations]", transaction: tx);

                    tx.Commit();
                }

                action(c);
            }
        }
    }
}
