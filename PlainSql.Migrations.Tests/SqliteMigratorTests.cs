using System.Data;
using System.Data.SQLite;

namespace PlainSql.Migrations.Tests
{
    public class SqliteMigratorTests : AbstractMigratorTests
    {
        protected override IDbConnection Connection
        {
            get
            {
                var c = new SQLiteConnection("Data Source=:memory:;");
                c.Open();

                return c;
            }
        }
    }
}
