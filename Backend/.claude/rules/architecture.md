# Architecture boundaries and naming

KidsPartyVenue is a single deployable with clean-architecture layers. There are no architecture tests; the `arch-reviewer` agent checks these rules.

## Layer dependencies

- **Domain** depends on nothing: no EF Core, no ASP.NET, no NetTopologySuite unless a geo type is truly part of the model. Prefer plain coordinates or a value object in Domain, and map them to `Point` in Infrastructure.
- **Application** depends on Domain only. It defines the abstractions (`IUnitOfWork`, `IDbConnectionFactory`, `ICacheService`, `IDateTimeProvider`, and later `IGeocoder` and `IVenueQueryParser`), and Infrastructure implements them.
- **Presentation** depends on Application (and so on Domain). It never depends on Infrastructure.
- **Infrastructure** depends on Application and Presentation. It is where everything is wired together (`InfrastructureConfiguration.AddInfrastructure`). EF configurations, Dapper, HybridCache, external HTTP clients (geocoding, Anthropic) and auth all live here.
- **API** is the host only: `Program.cs`, middleware, health checks, and migration-on-startup in Development. No business logic.

## External services

- Every external dependency (geocoder, LLM, email, and so on) sits behind an Application-layer interface, with its implementation in Infrastructure. Handlers never use `HttpClient` or SDK types directly.
- LLM output is untrusted input. Validate it like a request DTO before using it. The model never gets database access.

## Naming and shape conventions

| Type | Rules |
|---|---|
| Command | `public sealed record *Command` implementing `ICommand`/`ICommand<T>` |
| Command handler | `internal sealed class *CommandHandler` |
| Query | `public sealed record *Query` implementing `IQuery<T>` |
| Query handler | `internal sealed class *QueryHandler` |
| Query response | `public sealed record *Response` (next to the query) |
| Validator | `internal sealed class *Validator : AbstractValidator<TCommand>` |
| Entity | inherits `Entity`, **has a private parameterless constructor**, private setters, a static `Create(...)` factory returning `Result<T>` |
| Errors | `public static class <FeatureArea>Errors` exposing `Error` instances |
| Repository | interface in Domain (`I<Entity>Repository`), `internal sealed` implementation in Infrastructure |
| EF configuration | `internal sealed class <Entity>Configuration : IEntityTypeConfiguration<T>` in `Infrastructure/<FeatureArea>/` |
| Endpoint | `internal sealed class <UseCase> : IEndpoint` (Presentation) |

## Vertical slice folder layout

- **Application**: `src/KidsPartyVenue.Application/{FeatureArea}/{UseCase}/`. For example, `Venues/CreateVenue/CreateVenueCommand.cs`, `CreateVenueCommandHandler.cs` and `CreateVenueCommandValidator.cs`. Query slices hold `*Query.cs`, `*QueryHandler.cs` and `*Response.cs`.
- **Domain**: `src/KidsPartyVenue.Domain/{FeatureArea}/` (entity, `*Errors`, repository interface).
- **Presentation**: `src/KidsPartyVenue.Presentation/{FeatureArea}/{UseCase}.cs`, one endpoint class per use case.
- **Infrastructure**: `src/KidsPartyVenue.Infrastructure/{FeatureArea}/` (EF configuration, repository).

## Reads vs writes

- Commands load and modify state through repositories, then call `IUnitOfWork.SaveChangesAsync`.
- Queries read with Dapper through `IDbConnectionFactory` (backed by `NpgsqlDataSource`). Don't use EF in query handlers, and don't use Dapper writes in command handlers.
- Geo queries (PostGIS `ST_DWithin` / `IsWithinDistance`) can use EF LINQ with NetTopologySuite or Dapper SQL. In both cases the column is `geography(Point, 4326)`, distances are in **meters**, and the column has a GiST index.
