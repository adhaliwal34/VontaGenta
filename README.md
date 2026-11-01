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

1. Set up Postgres (an example is given below).
2. Load the schema from [DatabaseBackups/vonta_genta_db_schema.sql](DatabaseBackups/vonta_genta_db_schema.sql) into the Postgres DB, for example `psql -h <host> -U vonta_genta_db_user -d vonta_genta_db -f DatabaseBackups/vonta_genta_db_schema.sql`.
3. Copy `src/VontaGenta/appsettings.example.json` to `src/VontaGenta/appsettings.json` (and `appsettings.Development.json` if you want) and fill in your connection string, AWS SES credentials and hCaptcha secret. These files are gitignored, so keep secrets out of tracked files.
4. Run the app:

   ```bash
   dotnet run --project src/VontaGenta/VontaGenta.csproj
   ```

Registration is controlled by the `RegistrationEnabled` setting.

### Example Postgres setup

These are the steps for creating a Postgres DB in a docker container in a Ubuntu VM. Any working Postgres instance will do. This is the setup I use.

1. SSH to the VM.
2. Create a docker volume so that data is retained after container shutdown and restart: `sudo docker volume create pg_data`
3. In one directory, create the `.env` file. Do not commit it.

   ```text
   POSTGRES_USER=postgres
   POSTGRES_PASSWORD=<choose-a-password>
   ```

4. In the same directory, create the `init-db.sql` file. Use its DB name, user name and password in the connection string in step 3 of the "Getting started" section above. This init script only runs once, when the data volume is empty.

   ```sql
   -- Create the application database
   CREATE DATABASE vonta_genta_db;

   -- Create the application user (non-superuser)
   CREATE USER vonta_genta_db_user WITH PASSWORD '<choose-a-password>';

   -- Grant privileges on the database
   GRANT ALL PRIVILEGES ON DATABASE vonta_genta_db TO vonta_genta_db_user;

   -- Connect to the app database to configure schema permissions
   \c vonta_genta_db

   -- Ensure user can create tables in the public schema
   GRANT ALL ON SCHEMA public TO vonta_genta_db_user;
   ```

5. From that directory, run the container:

   ```bash
   sudo docker run --name vonta_postgres      --env-file .env      -v $(pwd)/init-db.sql:/docker-entrypoint-initdb.d/init-db.sql      -v pg_data:/var/lib/postgresql/data      -p 5432:5432      --restart always      -d postgres:17
   ```

The `-p 5432:5432` option exposes Postgres to all the VM's network interfaces. Restrict access with a firewall rule or bind to a specific IP if the VM is reachable beyond your machine.

## Tests

The xUnit tests are in [tests/VontaGenta.Tests](tests/VontaGenta.Tests):

   ```bash
   dotnet test VontaGenta.sln
   ```

## Licence

Licensed under the [Apache License, Version 2.0](LICENSE). Copyright 2026 Ajit Singh Dhaliwal.

Apache-2.0 grants no trademark rights, so please don't use the name "VontaGenta" for forks.

## How this was built

VontaGenta is a portfolio project. I designed the architecture and the domain model, and I directed and reviewed all of the work. I used AI assistance (Claude Code) throughout development, including writing code, refactoring, tests and documentation. Commits that include AI assistance carry a `Co-Authored-By` trailer.

## Third-party software

Dependencies keep their own licences. Vendored front-end libraries under `wwwroot/lib` ship with their licence files.
