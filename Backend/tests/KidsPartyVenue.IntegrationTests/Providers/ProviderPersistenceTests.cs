using System.Data.Common;
using AwesomeAssertions;
using Dapper;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Database;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class ProviderPersistenceTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private const string UniqueViolation = "23505";
    private const string ForeignKeyViolation = "23503";
    private const string RestrictViolation = "23001";

    // Beograd has no districts in the seed, so a provider there needs none
    private const string CityWithoutDistrictsSlug = "beograd";

    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task Insert_ShouldStoreEveryField_WhenProviderIsSaved()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();
        Provider provider = Provider.Create(
            UniqueSlug(), Faker.Company.CompanyName(), ProviderCategory.Venue, Faker.Lorem.Paragraph(), Faker.Address.StreetAddress(),
            district.CityId, district, cityHasDistricts: true, GeoCoordinate.Create(45.2389, 19.8441).Value,
            PhoneNumber.Create("064 123 4567").Value, isClaimed: true, UtcNowInMicroseconds()).Value;

        // Act
        await SaveAsync(provider);

        // Assert: read in a new scope, so the provider comes from the database and not the change tracker
        using IServiceScope scope = _factory.Services.CreateScope();
        Provider? saved = await scope.ServiceProvider.GetRequiredService<IProviderRepository>()
            .GetByIdAsync(provider.Id, CancellationToken.None);
        saved.Should().NotBeNull();
        saved.Should().BeEquivalentTo(provider);
        saved!.Phone!.Value.Should().Be("+381641234567");
    }

    [Fact]
    public async Task Insert_ShouldStoreNulls_WhenOptionalFieldsAreMissing()
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());

        // Act
        await SaveAsync(provider);

        // Assert
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == provider.Id);
        saved.DistrictId.Should().BeNull();
        saved.Phone.Should().BeNull();
        saved.ClaimedAtUtc.Should().BeNull();
        saved.SuspendedAtUtc.Should().BeNull();
        saved.Status.Should().Be(ProviderStatus.Draft);
    }

    [Fact]
    public async Task Insert_ShouldFail_WhenSlugIsTaken()
    {
        // Arrange
        string slug = UniqueSlug();
        await SaveAsync(await CreateInCityWithoutDistrictsAsync(slug));
        Provider duplicate = await CreateInCityWithoutDistrictsAsync(slug);

        // Act
        Func<Task> act = () => SaveAsync(duplicate);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, UniqueViolation);
        error.ConstraintName.Should().Be("ix_providers_slug");
    }

    [Fact]
    public async Task Insert_ShouldFail_WhenCityDoesNotExist()
    {
        // Arrange
        Provider provider = Provider.Create(
            UniqueSlug(), Faker.Company.CompanyName(), ProviderCategory.Venue, null, Faker.Address.StreetAddress(),
            Guid.CreateVersion7(), null, cityHasDistricts: false, GeoCoordinate.Create(44.8125, 20.4612).Value,
            null, isClaimed: false, DateTime.UtcNow).Value;

        // Act
        Func<Task> act = () => SaveAsync(provider);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, ForeignKeyViolation);
        error.ConstraintName.Should().Be("fk_providers_cities_city_id");
    }

    [Fact]
    public async Task Insert_ShouldFail_WhenDistrictDoesNotExist()
    {
        // Arrange
        City city = await DbContext.Cities.AsNoTracking().SingleAsync(c => c.Slug == CityWithoutDistrictsSlug);
        District unknown = District.Create(city.Id, "nepostojeci", city.Name).Value;
        Provider provider = Provider.Create(
            UniqueSlug(), Faker.Company.CompanyName(), ProviderCategory.Venue, null, Faker.Address.StreetAddress(),
            city.Id, unknown, cityHasDistricts: true, GeoCoordinate.Create(44.8125, 20.4612).Value,
            null, isClaimed: false, DateTime.UtcNow).Value;

        // Act
        Func<Task> act = () => SaveAsync(provider);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, ForeignKeyViolation);
        error.ConstraintName.Should().Be("fk_providers_districts_district_id");
    }

    [Fact]
    public async Task DeleteCity_ShouldFail_WhenAProviderUsesIt()
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        await SaveAsync(provider);

        // Act
        Func<Task> act = () => DeleteInRolledBackTransactionAsync("DELETE FROM cities WHERE id = {0}", provider.CityId);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, RestrictViolation);
        error.ConstraintName.Should().Be("fk_providers_cities_city_id");
    }

    [Fact]
    public async Task DeleteDistrict_ShouldFail_WhenAProviderUsesIt()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();
        Provider provider = Provider.Create(
            UniqueSlug(), Faker.Company.CompanyName(), ProviderCategory.Venue, null, Faker.Address.StreetAddress(),
            district.CityId, district, cityHasDistricts: true, GeoCoordinate.Create(45.2389, 19.8441).Value,
            null, isClaimed: false, DateTime.UtcNow).Value;
        await SaveAsync(provider);

        // Act
        Func<Task> act = () => DeleteInRolledBackTransactionAsync("DELETE FROM districts WHERE id = {0}", district.Id);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, RestrictViolation);
        error.ConstraintName.Should().Be("fk_providers_districts_district_id");
    }

    [Fact]
    public async Task Location_ShouldBeFoundByRadiusInMeters()
    {
        // Arrange: about 10 km apart, north to south
        GeoCoordinate point = GeoCoordinate.Create(44.8125, 20.4612).Value;
        Provider near = await CreateInCityWithoutDistrictsAsync(UniqueSlug(), point);
        Provider far = await CreateInCityWithoutDistrictsAsync(UniqueSlug(), GeoCoordinate.Create(44.9025, 20.4612).Value);
        await SaveAsync(near, far);

        // Act
        using IServiceScope scope = _factory.Services.CreateScope();
        await using DbConnection connection = await scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>()
            .OpenConnectionAsync();
        IEnumerable<Guid> found = await connection.QueryAsync<Guid>(
            """
            SELECT id
            FROM providers
            WHERE id = ANY(@Ids)
              AND ST_DWithin(location, ST_MakePoint(@Lng, @Lat)::geography, @Meters)
            """,
            new { Ids = new[] { near.Id, far.Id }, Lng = point.Longitude, Lat = point.Latitude, Meters = 1000 });

        // Assert
        found.Should().Equal(near.Id);
    }

    [Fact]
    public async Task Location_ShouldHaveAGistIndex()
    {
        // Act
        using IServiceScope scope = _factory.Services.CreateScope();
        await using DbConnection connection = await scope.ServiceProvider.GetRequiredService<IDbConnectionFactory>()
            .OpenConnectionAsync();
        string? definition = await connection.QuerySingleOrDefaultAsync<string>(
            "SELECT indexdef FROM pg_indexes WHERE tablename = 'providers' AND indexname = 'ix_providers_location'");

        // Assert
        definition.Should().Contain("USING gist (location)");
    }

    // timestamptz keeps microseconds, so a time with finer ticks wouldn't come back equal
    private static DateTime UtcNowInMicroseconds()
    {
        DateTime utcNow = DateTime.UtcNow;

        return new DateTime(utcNow.Ticks - utcNow.Ticks % TimeSpan.TicksPerMicrosecond, DateTimeKind.Utc);
    }

    private static string UniqueSlug() => $"provider-{Guid.CreateVersion7():N}";

    private async Task<District> GetNoviSadDistrictAsync()
    {
        City noviSad = await DbContext.Cities.AsNoTracking().SingleAsync(c => c.Slug == DevelopmentSeeder.NoviSadSlug);

        return await DbContext.Districts.AsNoTracking().FirstAsync(d => d.CityId == noviSad.Id);
    }

    private async Task<Provider> CreateInCityWithoutDistrictsAsync(string slug, GeoCoordinate? location = null)
    {
        City city = await DbContext.Cities.AsNoTracking().SingleAsync(c => c.Slug == CityWithoutDistrictsSlug);

        return Provider.Create(
            slug, Faker.Company.CompanyName(), ProviderCategory.Venue, null, Faker.Address.StreetAddress(),
            city.Id, null, cityHasDistricts: false, location ?? GeoCoordinate.Create(44.8125, 20.4612).Value,
            null, isClaimed: false, DateTime.UtcNow).Value;
    }

    // Each save gets its own scope, so a failed save leaves nothing tracked for the next one
    private async Task SaveAsync(params Provider[] providers)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        IProviderRepository repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        foreach (Provider provider in providers)
        {
            repository.Insert(provider);
        }

        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
    }

    // Rolled back, so a missing foreign key can't delete seed data that other tests need
    private async Task DeleteInRolledBackTransactionAsync(string sql, Guid id)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        KidsPartyVenueDbContext context = scope.ServiceProvider.GetRequiredService<KidsPartyVenueDbContext>();
        await using IDbContextTransaction transaction = await context.Database.BeginTransactionAsync();
        await context.Database.ExecuteSqlRawAsync(sql, id);
        await transaction.RollbackAsync();
    }

    private static async Task<PostgresException> ShouldFailInDatabase(Func<Task> act, string sqlState)
    {
        Exception exception = (await act.Should().ThrowAsync<Exception>()).Which;
        PostgresException error = (exception as PostgresException ?? exception.InnerException as PostgresException)!;
        error.Should().NotBeNull();
        error.SqlState.Should().Be(sqlState);

        return error;
    }
}
