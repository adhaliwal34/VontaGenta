# VontaGenta

A multi-organisation project and finance tracker built with ASP.NET Core MVC (.NET 10) and PostgreSQL. It tracks clients, projects, cost entries, revenues and timesheets, and users only see data for the organisations they belong to.

## Tech stack

- ASP.NET Core MVC on .NET 10
- PostgreSQL with Dapper for application data
- Entity Framework Core and ASP.NET Identity for users and roles only
- Serilog for logging, AWS SES for email, hCaptcha for sign-up protection

## Architecture

`Controllers` → `Services` → `Data` (repositories) → PostgreSQL.

- Each feature has a file set in every layer, with manual extension-method mapping (no AutoMapper).
- Services return result DTOs with an `IsSuccess` flag and an `Error` string instead of throwing.
- Authorisation checks live in the data layer as SQL `userorgs` membership checks.

See [CLAUDE.md](CLAUDE.md) for a fuller description.

## Getting started

Prerequisites: the .NET SDK pinned in [global.json](global.json) and PostgreSQL.

1. Create a database and load the schema from [DatabaseBackups/vonta_genta_db_schema.sql](DatabaseBackups/vonta_genta_db_schema.sql).
2. Copy `appsettings.example.json` to `appsettings.json` (and `appsettings.Development.json` if you want) and fill in your connection string, AWS SES credentials and hCaptcha secret. These files are gitignored, so keep secrets out of tracked files.
3. Run the app:

   ```bash
   dotnet run --project VontaGenta.csproj
   ```

Registration is controlled by the `RegistrationEnabled` setting.

## Tests

The xUnit tests live in a sibling repository, `VontaGentaTest`, which references this project:

```bash
dotnet test ../VontaGentaTest/VontaGentaTest.csproj
```

## Licence

Licensed under the [Apache License, Version 2.0](LICENSE). Copyright 2026 Ajit Singh Dhaliwal.

Apache-2.0 grants no trademark rights, so please don't use the name "VontaGenta" for forks.

## How this was built

VontaGenta is a portfolio project. I designed the architecture and the domain model, and I directed and reviewed all of the work. I used AI assistance (Claude Code) throughout development, including writing code, refactoring, tests and documentation. Commits that include AI assistance carry a `Co-Authored-By` trailer.

## Third-party software

Dependencies keep their own licences. Vendored front-end libraries under `wwwroot/lib` ship with their licence files.
