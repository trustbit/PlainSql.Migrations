# PlainSql.Migrations

![Build Status](https://github.com/trustbit/PlainSql.Migrations/actions/workflows/test.yml/badge.svg?branch=master)
[![Coverage Status](https://coveralls.io/repos/github/trustbit/PlainSql.Migrations/badge.svg?branch=master)](https://coveralls.io/github/trustbit/PlainSql.Migrations?branch=master)

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

`PlainSql.Migrations` offers a .NET Global tool that can be installed like so `dotnet tool install --global PlainSql.Migrations.Tool` and then executed from the terminal like so `migrate -c "Uid=myuser;Pwd=password1;Host=localhost;Database=northwind;" -d postgres`. 

## Database Support

* SQLite
* MS SQL
* PostgreSQL

## Concurrency

Migrations are executed in a single transaction with isolation level "Serializable". This usually
means that executing migrations is concurrency-safe. For details on which SQL statements are supported
by this transaction level, please refer to the documentation of your database technology.
