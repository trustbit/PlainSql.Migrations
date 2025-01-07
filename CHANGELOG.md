# Change Log

All notable changes to this project will be documented in this file. See [versionize](https://github.com/saintedlama/versionize) for commit guidelines.

## 3.0.0 (2025-01-07)

* ditched .NET 3.1, .NET 5.0 support for tool
* enabled .NET 8.0, .NET 9.0 support for tool
* upgraded Npgsql to 9 version
* Add GitHub actions CI flow

## 2.0.0 (2022-04-26)

* ditched .NET 2.1 support
* enabled .NET 6.0 support
* upgraded Npgsql to 6 version
* Using UTC timestamps instead of zoned/offset

## 1.4.0 (2022-1-24)

* Bump .NET versions

## 1.3.2 (2020-1-17)

### Bug Fixes

* use transaction level serializable (#4)

## 1.3.1 (2019-12-18)

### Bug Fixes

* sort migration scripts alphabetically (#3)

## 1.3.0 (2019-12-16)

### Features

* add PostgreSQL support (#2)

## 1.2.0 (2019-2-26)

### Bug Fixes

* rename, simplify and stabilize migration options and script processing

### Features

* add script processing and configuration builder

## 1.1.1 (2018-11-28)

## 1.1.0 (2018-11-28)

### Features

* add support for sqlite

## 1.0.0 (2018-11-28)

### Features

* update project to dotnet standard 2.0, prepare nuget data, add coverage, fix bugs in migrator and add more documentation
