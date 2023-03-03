# PlainSql.Migrations

[![Build status](https://ci.appveyor.com/api/projects/status/tuai6peo17rk4sp1/branch/master?svg=true)](https://ci.appveyor.com/project/Softwarepark/plainsql-migrations/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/Softwarepark/PlainSql.Migrations/badge.svg?branch=master)](https://coveralls.io/github/Softwarepark/PlainSql.Migrations?branch=master)
[![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-yellow.svg)](https://conventionalcommits.org)

Execute migration scripts written in plain old SQL. Only executes each migration once.

## Installation

```bash
dotnet add package PlainSql.Migrations
```

## Usage

`PlainSql.Migrations` provide a script loader and a migrator that interact to load and execute migration scripts.

```csharp
using PlainSql.Migrations;

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

`MigrationScriptsLoader.FromDirectory` orders the migration scripts alphabetically
(using `System.StringComparer.InvariantCulture`).

## Global Tool

`PlainSql.Migrations` offers a .NET Global tool that can be installed like
so `dotnet tool install --global PlainSql.Migrations.Tool` and then executed from the terminal like
so `migrate -c "Uid=myuser;Pwd=password1;Host=localhost;Database=northwind;" -d postgres`.

## Creating Migration Files

`PlainSql.Migrations` also includes a .NET Global tool to generate timestamped migration files in the format
of `yyyyMMddHHmmss_DescriptionOfTheMigration.sql`. 

### Usage

#### Installation

Install with `dotnet tool install --global PlainSql.Migrations.Generator`.

#### Generating Files
Use the `generate-migration` command.

```
$ generate-migration "add phone number to user table"
[10:42:21 INF] Creating migration file 20230302104221_AddPhoneNumberToUserTable.sql in ./MigrationScripts
[10:42:21 INF] Sucessfully created 20230302104221_AddPhoneNumberToUserTable.sql!
```

A different location for the migration scripts folder can be supplied with the `-m` option:
```
$ generate-migration "add phone number to user table" -m "./migrations"
[10:44:54 INF] Creating migration file 20230302104454_AddPhoneNumberToUserTable.sql in ./migrations
[10:44:54 INF] Sucessfully created 20230302104454_AddPhoneNumberToUserTable.sql!
```

## Database Support

* SQLite
* MS SQL
* PostgreSQL

## Concurrency

Migrations are executed in a single transaction with isolation level "Serializable". This usually
means that executing migrations is concurrency-safe. For details on which SQL statements are supported
by this transaction level, please refer to the documentation of your database technology.
