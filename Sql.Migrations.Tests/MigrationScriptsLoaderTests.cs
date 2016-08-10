using Xunit;
using System.IO;

namespace Sql.Migrations.Tests
{
    public class MigrationScriptsLoaderTests
    {
        static readonly string ResourceDirectory;
        static MigrationScriptsLoaderTests()
        {
            const string resource = "resources";
            var dir = Path.Combine(Directory.GetCurrentDirectory(), resource);
            if (!Directory.Exists(dir))
                dir = Path.Combine(Directory.GetCurrentDirectory(), "Sql.Migrations.Tests", resource);

            ResourceDirectory = dir;
        }

        [Fact]
        public void Searching_with_non_existing_extension_should_lead_to_no_result()
        {
            Assert.Empty(MigrationScriptsLoader.FromDirectory(ResourceDirectory, "*.foo"));
        }

        [Fact]
        public void Searching_with_the_correct_suffix_should_lead_to_two_entries()
        {
            Assert.Collection(MigrationScriptsLoader.FromDirectory(ResourceDirectory, "*.sql"),
                x =>
                {
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