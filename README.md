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
2. Copy `src/VontaGenta/appsettings.example.json` to `src/VontaGenta/appsettings.json` (and `appsettings.Development.json` if you want) and fill in your connection string (using the `vonta_genta_app_user` and its password), AWS SES credentials and hCaptcha secret. These files are gitignored, so keep secrets out of tracked files.
3. Run the app:

   ```bash
   dotnet run --project src/VontaGenta/VontaGenta.csproj
   ```

Registration is controlled by the `RegistrationEnabled` setting. No user is registered on initial start up.

### Example Postgres setup

These are the steps for creating a Postgres DB in a docker container in a Ubuntu VM that is run using VirtualBox and Vagrant. Any working Postgres instance will do. This is the setup AJ uses.

1. Copy the file `DatabaseSetup/.env.example` to `DatabaseSetup/.env` and add your chosen passwords.
2. Map the folder `DatabaseSetup` to the folder `/vonta-db-setup` in the VM by adding these lines to the Vagrantfile

   ```ruby
   Vagrant.configure("2") do |config|
     # ... existing config ...
     config.vm.synced_folder "<path to folder>/DatabaseSetup",
                             "/vonta-db-setup"
   end
   ```

3. Start the VM by changing directory to the folder with the Vagrantfile and using `vagrant up`.
4. SSH to the VM using `vagrant ssh`
5. Create a docker volume so that data is retained after container shutdown and restart: `sudo docker volume create pg_data`
6. Start the container:

   ```bash
   sudo docker run --name vonta_postgres --env-file /vonta-db-setup/.env \
     -v /vonta-db-setup/01-init-db.sql:/docker-entrypoint-initdb.d/01-init-db.sql:ro \
     -v /vonta-db-setup/02-schema-and-seed.sql:/docker-entrypoint-initdb.d/02-schema-and-seed.sql:ro \
     -v /vonta-db-setup/03-app-user.sql:/docker-entrypoint-initdb.d/03-app-user.sql:ro \
     -v /vonta-db-setup/vonta_genta_db_schema.sql:/vonta-sql/vonta_genta_db_schema.sql:ro \
     -v /vonta-db-setup/vonta_genta_db_seed.sql:/vonta-sql/vonta_genta_db_seed.sql:ro \
     -v pg_data:/var/lib/postgresql \
     -p 5432:5432 --restart always -d postgres:18
   ```

The `-p 5432:5432` option exposes Postgres to all the VM's network interfaces. Restrict access with a firewall rule or bind to a specific IP if the VM is reachable beyond your machine.

7. Run `sudo docker logs vonta_postgres | grep "PostgreSQL init process complete"` and look for the line "PostgreSQL init process complete". If the line is missing, check the rest of the output from `sudo docker logs vonta_postgres` and fix any errors raised. Delete the docker container and volume before trying again. See step 3 in "Example of starting with a clean Postgres db" for how to do this. Continue from step 5 of this section.

8. Continue from step 2 of the "Getting started" section above.

### Example of starting with a clean Postgres db

If you already have a Postgres db set up as described in the "Example Postgres setup" section and you want a fresh db, follow these instructions.

1. Start the VM using `vagrant up`
2. SSH to the VM using `vagrant ssh`
3. Execute the following lines to stop the existing docker container, delete it, and delete the volume

   ```bash
   sudo docker stop vonta_postgres
   sudo docker rm vonta_postgres
   sudo docker volume rm pg_data
   ```
4. Continue from step 5 of the "Example Postgres setup" section.

## Tests

The xUnit tests are in [tests/VontaGenta.Tests](tests/VontaGenta.Tests):

   ```bash
   dotnet test VontaGenta.sln
   ```

## Licence

Licensed under the [Apache License, Version 2.0](LICENSE). Copyright 2026 Ajit Singh Dhaliwal.

Apache-2.0 grants no trademark rights, so please don't use the name "VontaGenta" for forks.

## How this was built

The initial code was written by AJ Dhaliwal and represents years of his work and study in the field of Project Management software. This repo was created as a portfolio project and an opportunity to experiment with AI-Assisted coding. Commits that include AI assistance carry a `Co-Authored-By` trailer.

## Third-party software

Dependencies keep their own licences. Vendored front-end libraries under `wwwroot/lib` ship with their licence files.
