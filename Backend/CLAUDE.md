# KidsPartyVenue backend

ASP.NET Core (.NET 10) API for finding and booking kids' party venues. Single-deployable clean architecture with CQRS-style vertical slices. Claude is started from this `Backend/` folder; all paths below are relative to it.

## Layout

| Project | Role | References |
|---|---|---|
| `src/KidsPartyVenue.Domain` | Entities, value objects, `Result`/`Error` | nothing |
| `src/KidsPartyVenue.Application` | Commands/queries + handlers, validators, abstractions (`IUnitOfWork`, `IDbConnectionFactory`, `ICacheService`, `IDateTimeProvider`) | Domain |
| `src/KidsPartyVenue.Presentation` | Minimal API endpoints (`IEndpoint`), `ApiResults` | Application |
| `src/KidsPartyVenue.Infrastructure` | EF Core `KidsPartyVenueDbContext`, Dapper, caching, auth, external services; DI composition (`AddInfrastructure`) | Application, Presentation |
| `src/KidsPartyVenue.API` | Host: `Program.cs`, middleware, Serilog, health checks, dev-only auto-migrations | Infrastructure |
| `tests/KidsPartyVenue.UnitTests`, `tests/KidsPartyVenue.IntegrationTests` | To be added: xUnit; integration tests use Testcontainers (`postgis/postgis`) | — |

Detailed rules: [.claude/rules/architecture.md](.claude/rules/architecture.md), [.claude/rules/coding-style.md](.claude/rules/coding-style.md).

## Stack

- **Database**: PostgreSQL 18 + PostGIS 3.6 (`postgis/postgis:18-3.6`) via `Npgsql.EntityFrameworkCore.PostgreSQL`. One singleton `NpgsqlDataSource` is shared by Dapper (`IDbConnectionFactory`), EF Core and the health check. EF Core for writes, Dapper for reads. snake_case names (`UseSnakeCaseNamingConvention`).
- **Caching**: `HybridCache` (in-memory only, no Redis) behind `ICacheService` (`GetOrCreateAsync` / `SetAsync` / `RemoveAsync`). The default expiration comes from `Caching:DefaultExpirationInMinutes`.
- **Logging**: Serilog, configured from the `Serilog` section in appsettings, writing to the console.
- **IDs**: `Guid.CreateVersion7()` stored as `uuid`.

## Configuration

- `ConnectionStrings:Database` is the only connection string.
  - **On the host** (`dotnet run`, VS "http"/"https" profiles): `Host=localhost;Port=5433` (compose maps 5433 to 5432 in the container).
  - **Inside compose**: `docker-compose.yml` overrides it with `ConnectionStrings__Database` pointing at `Host=kidspartyvenue.database;Port=5432`. Environment variables win over appsettings.
- Database data lives in a bind mount at `.containers/db` (gitignored and dockerignored). Delete that folder to reset the dev database.
- Secrets go in `.env` (gitignored; `.env.example` is committed) or user secrets. Never commit them to appsettings.

## Commands

```powershell
dotnet build KidsPartyVenue.slnx
dotnet test KidsPartyVenue.slnx                          # once test projects exist
docker compose up -d kidspartyvenue.database            # only the db, then run the API on the host
dotnet run --project src/KidsPartyVenue.API              # uses localhost:5433
docker compose up -d --build                             # db (PostGIS) + api
docker compose logs -f kidspartyvenue.api
```

- Health: `GET /health`. In Development, Scalar UI is at `/scalar` and migrations run on startup.
- Migrations: use the `db-migration` skill. Never hand-edit files in `Database/Migrations` (a hook blocks this).
- New use case: use the `add-vertical-slice` skill. Before committing, run the `arch-reviewer` agent.
