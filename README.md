# Sql.Migrations

[![Build status](https://ci.appveyor.com/api/projects/status/m4313sgopt1arwij?svg=true)](https://ci.appveyor.com/project/saintedlama/plainsql-migrations)
[![Coverage Status](https://coveralls.io/repos/github/Softwarepark/PlainSql.Migrations/badge.svg?branch=master)](https://coveralls.io/github/Softwarepark/PlainSql.Migrations?branch=master)
[![Conventional Commits](https://img.shields.io/badge/Conventional%20Commits-1.0.0-yellow.svg)](https://conventionalcommits.org)

Execute migration scripts written in plain old SQL. Only executes each migration once.

## Installation

```bash
dotnet  add package Sql.Migrations
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

## Database Support

* SQLite
* MS SQL
