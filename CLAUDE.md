# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

ASP.NET Core MVC app on .NET 10 (SDK pinned in [global.json](global.json)) with PostgreSQL.

- Build: `dotnet build VontaGenta.sln`
- Run: `dotnet run --project src/VontaGenta/VontaGenta.csproj`
- Tests (xUnit, in [tests/VontaGenta.Tests](tests/VontaGenta.Tests)): `dotnet test VontaGenta.sln`
- Single test: `dotnet test tests/VontaGenta.Tests/VontaGenta.Tests.csproj --filter "FullyQualifiedName~DapperHelperTests"`

- Format check: `dotnet format --verify-no-changes VontaGenta.sln` (run `dotnet format VontaGenta.sln` to fix). Keep it passing.

Linting uses the Roslyn analyzers, configured by `AnalysisLevel` and `EnforceCodeStyleInBuild` in [VontaGenta.csproj](src/VontaGenta/VontaGenta.csproj) and by [.editorconfig](.editorconfig). Analyzer findings show up as warnings in `dotnet build`, so check it for new ones. `Migrations` and `Areas/Identity` are marked as generated code and skipped. CA1848 (LoggerMessage delegates) is turned off on purpose. Line endings are LF (`.gitattributes` and `.editorconfig`), apart from `.sln`, `.bat` and `.cmd`, which are CRLF.

The database schema is kept in [DatabaseBackups/vonta_genta_db_schema.sql](DatabaseBackups/vonta_genta_db_schema.sql).

## Architecture

The web app is in `src/VontaGenta` and the xUnit tests in `tests/VontaGenta.Tests`. Paths below are relative to `src/VontaGenta`.

Layering: `Controllers` → `Services` → `Data` (repos) → PostgreSQL. Each feature (Clients, Projects, CostEntries, Revenues, Timesheets, Orgs, ...) has a file set in each layer, plus `Models/<Feature>` (view models), `Mapping/<Feature>Mappings.cs` (manual extension-method mapping, no AutoMapper) and `DependencyInjection/<Feature>ServicesExtensions.cs`. A new feature must be registered with an `Add<Feature>Services()` call in [Program.cs](src/VontaGenta/Program.cs).

### Data access: Dapper, not EF, for app data
- Entity Framework (`ApplicationDbContext`) is used only for ASP.NET Identity (`AppUser`, roles) and Identity UI under `Areas/Identity`.
- All domain queries are raw SQL through Dapper over an `NpgsqlConnection` held by `UnitOfWork` ([Data/UnitofWork.cs](src/VontaGenta/Data/UnitofWork.cs)). SQL uses lowercase Postgres table and column names (`clients`, `aspnetusers`, `userorgs`).
- Repos derive from `RepoBase`, which exposes `Connection` and `Transaction` from the unit of work. Services reach repos only through `IUnitOfWork` (for example `_unitOfWork.Clients`). Repos are created lazily inside `UnitOfWork`. Adding a repo means editing `IUnitOfWork`, `UnitOfWork` (field, property and lazy initialiser) and the feature's DI extension.
- Services open transactions explicitly (`BeginTransactionAsync` / `CommitAsync` / `RollbackAsync`) for writes.
- `Helpers/DapperHelper.cs` builds bulk-insert SQL.
- `Program.cs` sets `Npgsql.EnableLegacyTimestampBehavior`. Keep it when touching date/time code.

### Authorization lives in the data layer
Permission checks are hard-coded in SQL. Typically a service calls a repo `Allow*` method (for example `AllowGetClient`) that checks `userorgs` membership before fetching. Users see data only for orgs they belong to, and `BaseController.GetActiveOrgSetName` resolves the user's active org.

### Services return DTOs, not exceptions
Services return per-operation DTOs (`Services/<Feature>/*DTOs.cs`) with an `Error` string and an `IsSuccess` property. Controllers surface them through `BaseController.SetError` / `SetStatus` (TempData). Repos catch exceptions and use `ErrorHelpers.GetExceptionError`.

### Other notes
- Serilog writes to `logs/` and is configured in `appsettings.json`.
- Select lists for views are built by `ViewHelpers/SelectListHelpers.cs` via the `IHas*SelectLists` interfaces in `Models/Interfaces`.
- Registration is gated by the `RegistrationEnabled` setting, and hCaptcha and AWS SES email are wired in via the `Hcaptcha` and `Emails` services.
- `appsettings.*` is gitignored and holds the local DB connection string, AWS SES credentials and the hCaptcha secret. A fresh clone has no config, so you must create it yourself. Keep secrets out of tracked files.
