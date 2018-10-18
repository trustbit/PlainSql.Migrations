# Sql.Migrations

[![Build status](https://ci.appveyor.com/api/projects/status/s0h50wkhhy46xvwb?svg=true)](https://ci.appveyor.com/project/Nagelfar/sql-migrations)

Execute migration scripts written in plain old SQL. Only executes each migration once.

## Installation

```bash
dotnet  add package Sql.Migrations
```

## Usage

`Sql.Migration` provides a script loader and a migrator that interact to load and execute migration scripts.

```csharp
private void ExecuteMigrations(string connectionString, string environment)
{
    using (var connection = new SqlConnection(connectionString))
    {
        connection.Open();

        var migrationScripts = MigrationScriptsLoader.FromDirectory("./MigrationScripts");

        Migrator.ExecuteMigrations(connection, migrationScripts);

        Console.Writeline("Executed database migration scripts");
    }
}
```
