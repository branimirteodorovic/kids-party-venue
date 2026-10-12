---
name: add-vertical-slice
description: Add a new use case (command or query vertical slice) to the KidsPartyVenue backend, covering domain changes, command/query, handler, validator, endpoint, repository wiring and tests. Use when asked to implement a new feature, endpoint, or use case (e.g. venues, bookings, search).
---

# Add a vertical slice

A use case spans the Domain, Application, Presentation and (when persistence changes) Infrastructure projects. Paths are relative to `Backend/`. Once the first Venue slices exist, use them as the reference and mirror them.

## 1. Domain (only if new behavior or state is needed): `src/KidsPartyVenue.Domain/<FeatureArea>/`

- The entity inherits `Entity`, keeps a **private parameterless constructor** (for EF) and private setters, and has a static `Create(...)` factory that returns `Result<T>`. Behavior methods return `Result`.
- IDs are `Guid.CreateVersion7()`. Timestamps come from `IDateTimeProvider` (passed into `Create`) and are UTC.
- Business failures go in `public static class <FeatureArea>Errors`, for example `Error.NotFound("Venues.NotFound", "...")`.
- The repository interface `I<Entity>Repository` lives here.

## 2. Application: `src/KidsPartyVenue.Application/<FeatureArea>/<UseCase>/`

**Command slice** (3 files):

```csharp
// CreateVenueCommand.cs
public sealed record CreateVenueCommand(string Name, string Address, decimal PricePerHour) : ICommand<Guid>;

// CreateVenueCommandHandler.cs
internal sealed class CreateVenueCommandHandler(
    IVenueRepository venueRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider)
    : ICommandHandler<CreateVenueCommand, Guid>
{
    public async Task<Result<Guid>> Handle(CreateVenueCommand request, CancellationToken cancellationToken)
    {
        Result<Venue> result = Venue.Create(request.Name, request.Address, request.PricePerHour, dateTimeProvider.UtcNow);

        if (result.IsFailure)
        {
            return Result.Failure<Guid>(result.Error);
        }

        venueRepository.Insert(result.Value);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return result.Value.Id;
    }
}

// CreateVenueCommandValidator.cs (always create one)
internal sealed class CreateVenueCommandValidator : AbstractValidator<CreateVenueCommand>
{
    public CreateVenueCommandValidator()
    {
        RuleFor(c => c.Name).NotEmpty().MaximumLength(200);
        RuleFor(c => c.PricePerHour).GreaterThan(0);
    }
}
```

**Query slice** (3 files): a `*Query` record (`IQuery<TResponse>`), a `*QueryHandler` that uses **Dapper** (never EF), and a `*Response` record.

```csharp
await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

const string sql =
    $"""
     SELECT
         id AS {nameof(VenueResponse.Id)},
         name AS {nameof(VenueResponse.Name)},
         price_per_hour AS {nameof(VenueResponse.PricePerHour)}
     FROM venues
     WHERE id = @VenueId
     """;

VenueResponse? venue = await connection.QuerySingleOrDefaultAsync<VenueResponse>(
    new CommandDefinition(sql, new { request.VenueId }, cancellationToken: cancellationToken));

if (venue is null)
{
    return Result.Failure<VenueResponse>(VenueErrors.NotFound(request.VenueId));
}
```

Columns are snake_case. For a radius search, filter with `ST_DWithin(location, ST_MakePoint(@Lng, @Lat)::geography, @Meters)` (longitude first, distances in meters) and order by `ST_Distance(...)`.

Handlers and validators are registered automatically: Scrutor scans the Application assembly, and the Validation, Logging and ExceptionHandling decorators wrap every handler. No DI changes are needed for the handler.

**Caching**: if a query or external lookup should be cached, inject `ICacheService` (backed by HybridCache, in-memory only) and wrap the load in `GetOrCreateAsync`. HybridCache protects against stampedes: concurrent misses on the same key run the factory once.

```csharp
VenueResponse? venue = await cacheService.GetOrCreateAsync(
    $"venues:{request.VenueId}",
    async ct => await LoadVenueAsync(request.VenueId, ct),
    TimeSpan.FromMinutes(10),
    cancellationToken);
```

- Omit `expiration` to use `Caching:DefaultExpirationInMinutes`.
- When a command changes cached data, it calls `RemoveAsync` with the matching keys after `SaveChangesAsync`.
- Don't cache `null` or failure results unless you mean to. Return early before caching instead.

## 3. Presentation: `src/KidsPartyVenue.Presentation/<FeatureArea>/<UseCase>.cs`

```csharp
internal sealed class CreateVenue : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("venues", async (
            Request request,
            ICommandHandler<CreateVenueCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            Result<Guid> result = await handler.Handle(
                new CreateVenueCommand(request.Name, request.Address, request.PricePerHour),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .WithTags(Tags.Venues);
    }

    internal sealed class Request
    {
        public string Name { get; init; } = string.Empty;
        public string Address { get; init; } = string.Empty;
        public decimal PricePerHour { get; init; }
    }
}
```

- Endpoints are discovered automatically (`AddEndpoints` scans the Presentation assembly). GET endpoints bind route and query parameters directly instead of using a `Request` class.
- Keep tag names in `Presentation/Tags.cs` (create it if missing).
- Auth is deferred to Phase 7. After that, add `.RequireAuthorization(Permissions.X)` to every non-public endpoint.

## 4. Infrastructure (when persistence changes): `src/KidsPartyVenue.Infrastructure/<FeatureArea>/`

- `<Entity>Configuration : IEntityTypeConfiguration<T>`, applied from `KidsPartyVenueDbContext.OnModelCreating`.
- `internal sealed class <Entity>Repository(KidsPartyVenueDbContext context) : I<Entity>Repository`, **registered in `InfrastructureConfiguration.AddInfrastructure`**. Repositories are not auto-scanned.
- External services (for example `IGeocoder`) get an implementation here, registered with a typed `HttpClient` and options bound from configuration.
- Then generate a migration with the `db-migration` skill.

## 5. Tests (once the test projects exist)

- **Unit tests** in `tests/KidsPartyVenue.UnitTests/<FeatureArea>/`: cover every domain rule (success and failure paths) and validators. Use Bogus `Faker`, AwesomeAssertions and NSubstitute.
- **Integration tests** in `tests/KidsPartyVenue.IntegrationTests/<FeatureArea>/<UseCase>Tests.cs`: run against the `postgis/postgis` Testcontainer. Send the command or query through DI and assert the `Result` plus the database state. Geo queries must be tested here, not with mocks.
- Name tests `Method_ShouldOutcome_WhenCondition`.

## Verify

`dotnet build KidsPartyVenue.slnx`, then `dotnet test` (if test projects exist), then run the `arch-reviewer` agent.

## Checklist

- [ ] Command/query is a `public sealed record` named `*Command`/`*Query`
- [ ] Handler is `internal sealed`, named `*CommandHandler`/`*QueryHandler`, and returns `Result`
- [ ] Every command has an `internal sealed *Validator`
- [ ] Endpoint is `internal sealed : IEndpoint` with `.WithTags(...)`
- [ ] Queries use Dapper; commands use repositories + `IUnitOfWork`
- [ ] New repository registered in `InfrastructureConfiguration`
- [ ] Migration generated if the model changed
- [ ] Unit + integration tests added (once the test projects exist)
