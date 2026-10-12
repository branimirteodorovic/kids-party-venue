# Coding style

The build doesn't enforce these yet: there is no `Directory.Build.props`, `.editorconfig` or central package management. Follow them anyway. If those files get added (`TreatWarningsAsErrors`, analyzers, `Directory.Packages.props`), update this note.

## C#

- **`var` usage**: only when the type is obvious from the right-hand side (`var venue = new Venue()` is fine). Everything else uses explicit types: `Result<Venue> result = ...`, `string sql = ...`, `DateTime startsAtUtc = ...`.
- File-scoped namespaces, one type per file. Nested `Request` records inside endpoint classes are the accepted exception.
- Use primary constructors for handlers and services: `internal sealed class Foo(IDep dep) : IFoo`.
- Use collection expressions where possible: `List<X> items = [];`.
- Async all the way. Every handler and repository method takes a `CancellationToken` and passes it on.
- Use `IDateTimeProvider`, never `DateTime.UtcNow`, in Domain and Application code. Store UTC only: Npgsql maps `DateTime` with `Kind=Utc` to `timestamptz` and rejects `Unspecified`/`Local`.
- IDs: `Guid.CreateVersion7()`.
- No exceptions for business outcomes. Return `Result`/`Result<T>` with an `Error` from a `*Errors` class. Exceptions are only for infrastructure faults. `KidsPartyVenueException` wraps unexpected failures in cross-cutting code.
- Log with `ILogger<T>` and structured templates (`"Processing {VenueId}"`, never string interpolation). Serilog sinks are set in appsettings.

## SQL (Dapper on PostgreSQL)

- Write SQL as raw string literals (`"""`), with parameters passed as Dapper anonymous objects. Never interpolate values into SQL. Interpolating column aliases with `nameof(Response.Prop)` is fine because it happens at compile time.
- Tables and columns are snake_case and unquoted (`venues`, `price_per_hour`). EF creates them with `UseSnakeCaseNamingConvention`.
- Use Postgres syntax: `LIMIT/OFFSET`, `ILIKE`, `now()`, `= ANY(@Ids)` for arrays, and `ST_DWithin(location, ST_MakePoint(@Lng, @Lat)::geography, @Meters)` for radius search (longitude first).

## Caching

- Use `ICacheService` (backed by `HybridCache`, in-memory only) in Application code. Prefer `GetOrCreateAsync` over separate get and set calls. Don't inject `HybridCache`, `IMemoryCache` or `IDistributedCache` directly into handlers.
- Cache keys are `"<area>:<id-or-normalized-input>"`, for example `"geocode:<normalized address>"`. Always set an explicit expiration for external lookups.

## Tests (once the test projects exist)

- Use xUnit `[Fact]`/`[Theory]`, AwesomeAssertions `.Should()`, Bogus `Faker` for test data, and NSubstitute for mocks.
- Name tests `Method_ShouldOutcome_WhenCondition`, with `// Arrange`, `// Act` and `// Assert` comments.
- Integration tests run against a real `postgis/postgis` Testcontainer. Never use the EF in-memory provider or SQLite, because geo and SQL behavior would differ.
