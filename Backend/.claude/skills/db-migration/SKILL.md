---
name: db-migration
description: Create or update EF Core migrations for the KidsPartyVenue PostgreSQL/PostGIS database (KidsPartyVenueDbContext). Use for any schema change, such as a new entity, column, index, geo column, seed data, or fixing or removing a migration.
---

# EF Core migrations (PostgreSQL / PostGIS)

There is a single `KidsPartyVenueDbContext` in `src/KidsPartyVenue.Infrastructure`. Migrations live in `src/KidsPartyVenue.Infrastructure/Database/Migrations/`, and the startup project is the API. Run from `Backend/`:

```powershell
dotnet ef migrations add <Migration_Name> `
  --project src/KidsPartyVenue.Infrastructure `
  --startup-project src/KidsPartyVenue.API `
  --context KidsPartyVenueDbContext `
  --output-dir Database/Migrations
```

- Name migrations in `Underscore_Separated` style, for example `Initial_Create`, `Add_Venue_Location`.
- If `dotnet ef` is missing, run `dotnet tool install --global dotnet-ef`.
- The design-time build needs `ConnectionStrings:Database` to be present but doesn't connect to it. Development appsettings or user secrets are enough.
- To undo the latest **unapplied** migration, run `dotnet ef migrations remove` with the same `--project/--startup-project/--context` arguments. If it was already applied locally, either run `dotnet ef database update <PreviousMigration>` first, or reset the dev database: `docker compose down`, delete `.containers/db` (a bind mount, so `down -v` does **not** remove it), then `docker compose up -d`.
- `dotnet ef database update` and `migrations remove` run on the host, so they connect through the Development connection string (`localhost:5433`). The database container must be running.

## Rules

- **Never hand-edit** generated migration `.cs`/`.Designer.cs` files or `*ModelSnapshot.cs`. A PreToolUse hook blocks this. Change the entity or configuration and regenerate.
- Schema changes start in code: an entity property (private setter) plus an `IEntityTypeConfiguration<T>` in `src/KidsPartyVenue.Infrastructure/<FeatureArea>/`, picked up by `OnModelCreating`.
- Table and column names come out snake_case automatically (`UseSnakeCaseNamingConvention`). Don't name them manually unless you need to deviate.
- Use Postgres types: `uuid` for ids (`Guid.CreateVersion7()`), `timestamptz` for UTC `DateTime`, `text` / `varchar(n)` (set with `HasMaxLength`), and `numeric(p,s)` for money (`HasPrecision(10, 2)`). Never use SQL Server types (`nvarchar`, `datetime2`, `NEWID()`).
- Seed data goes in `HasData(...)` inside the configuration. Changing it requires a new migration.

## PostGIS

- The `postgis/postgis` image already ships the extension, but the model must still declare it with `modelBuilder.HasPostgresExtension("postgis");` in `OnModelCreating`, so the migration runs `CREATE EXTENSION`.
- NetTopologySuite must be turned on in **both** places, on the one shared data source (requires the `Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite` package):

```csharp
NpgsqlDataSource dataSource = new NpgsqlDataSourceBuilder(databaseConnectionString)
    .UseNetTopologySuite()                       // Dapper / ADO.NET
    .Build();

services.AddDbContext<KidsPartyVenueDbContext>(options => options
    .UseNpgsql(dataSource, o => o.UseNetTopologySuite())   // EF Core
    .UseSnakeCaseNamingConvention());
```
- Geo column:

```csharp
builder.Property(v => v.Location)
    .HasColumnType("geography (Point, 4326)");

builder.HasIndex(v => v.Location)
    .HasMethod("gist");
```

- Points are created with SRID 4326 and **longitude first**: `new Point(lng, lat) { SRID = 4326 }`.

## Applying migrations

- **Development**: applied automatically at startup (`app.ApplyMigrations()` in `Program.cs`, Development only), including inside `docker compose up`.
- **Integration tests**: applied against the PostGIS Testcontainer, so a broken migration fails the test run.
- **Production**: never on startup. Build a bundle (`dotnet ef migrations bundle ...`) and run it as a separate deploy step. This gets set up in the deploy phase.

## Verify

Run `dotnet build KidsPartyVenue.slnx`. Then either run `docker compose up -d --build` and check `GET /health` and the API logs for applied migrations, or run an integration test.
