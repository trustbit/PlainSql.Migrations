using Xunit;
using System.IO;

namespace Sql.Migrations.Tests
{
    public class MigrationScriptsLoaderTests
    {
        static readonly string ResourceDirectory = Path.Combine(Directory.GetCurrentDirectory(), "resources");

        [Fact]
        public void Searching_with_non_existing_extension_should_lead_to_no_result()
        {
            Assert.Empty(MigrationScriptsLoader.FromDirectory(ResourceDirectory, "*.foo"));
        }

        [Fact]
        public void Searching_with_the_correct_suffix_should_lead_to_two_entries()
        {
            Assert.Collection(MigrationScriptsLoader.FromDirectory(ResourceDirectory, "*.sql"),
                x => {
                    Assert.Equal("test1.sql", x.Name);
                    Assert.Equal("test1", x.Script);
                    },
                x =>
                {
                    Assert.Equal("test2.sql", x.Name);
                    Assert.Equal("test2", x.Script);
                }
            );
        }
    }
}