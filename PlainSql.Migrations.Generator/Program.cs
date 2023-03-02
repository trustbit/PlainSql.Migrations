using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using McMaster.Extensions.CommandLineUtils;
using Serilog;

namespace PlainSql.Migrations.Generator;

class Program
{
    public static int Main(string[] args) => CommandLineApplication.Execute<Program>(args);

    private const string Placeholder = "SELECT * FROM Students WHERE Name = 'Robert'); DROP TABLE Students;--";

    [Required]
    [Argument(0)]
    private string Description { get; }

    [Option(Description = "The folder where the migration scripts are located",
        ShortName = "m",
        LongName = "migrationscriptfolder")]
    private string MigrationScriptFolder { get; set; } = "./MigrationScripts";

    private void OnExecute()
    {
        Log.Logger = new LoggerConfiguration()
            .WriteTo.Console()
            .CreateLogger();

        try
        {
            CheckIfMigrationScriptFolderExists();

            var filename = CreateFilename();

            Log.Information("Creating migration file {filename} in {MigrationScriptFolder}",
                filename,
                MigrationScriptFolder);

            CreateMigrationFile(filename);

            Log.Information("Sucessfully created {filename}!", filename);
        }
        catch (Exception e)
        {
            Log.Error("There has been a problem creating the migration file: " + e.Message);
        }
    }

    private void CreateMigrationFile(string filename)
    {
        using var fs = File.Create(MigrationScriptFolder + "/" + filename);
        var info = new UTF8Encoding(true).GetBytes(Placeholder);
        fs.Write(info, 0, info.Length);
    }

    private void CheckIfMigrationScriptFolderExists()
    {
        if (!Directory.Exists(MigrationScriptFolder))
        {
            throw new InvalidOperationException(
                $"Error while loading migration scripts. Directory '{MigrationScriptFolder}' does not exist");
        }
    }

    private string CreateFilename()
    {
        var titleCaseDescription = CultureInfo.InvariantCulture.TextInfo.ToTitleCase(Description);
        var filename = DateTime.UtcNow.ToString("yyyyMMddHHmmss") + "_" +
                       Regex.Replace(titleCaseDescription, @"\s+", "") + ".sql";
        return filename;
    }
}
