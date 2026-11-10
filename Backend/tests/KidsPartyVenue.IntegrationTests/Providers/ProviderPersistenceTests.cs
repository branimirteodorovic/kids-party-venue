using System.Data.Common;
using AwesomeAssertions;
using Dapper;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Venues;
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
    private const string CheckViolation = "23514";

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

    [Theory]
    [InlineData(DepositType.None, null, null, 0)]
    [InlineData(DepositType.Percentage, 30, 3, 7)]
    [InlineData(DepositType.FixedAmount, 5000, 5, 14)]
    public async Task BookingPolicy_ShouldBeStored_WhenItIsSet(
        DepositType depositType, int? depositValue, int? depositDueDays, int freeCancellationDays)
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        BookingPolicy policy = BookingPolicy.Create(depositType, depositValue, depositDueDays, freeCancellationDays).Value;
        provider.ChangeBookingPolicy(policy, DateTime.UtcNow);

        // Act
        await SaveAsync(provider);

        // Assert
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == provider.Id);
        saved.BookingPolicy.Should().Be(policy);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangeBookingPolicy_ShouldBeStored_WhenProviderIsLoadedFromDatabase(bool hadPolicy)
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        if (hadPolicy)
        {
            provider.ChangeBookingPolicy(BookingPolicy.Create(DepositType.None, null, null, 7).Value, DateTime.UtcNow);
        }

        await SaveAsync(provider);
        BookingPolicy policy = BookingPolicy.Create(DepositType.FixedAmount, 5000, 5, 14).Value;

        // Act: load tracked in a new scope and replace the policy, as the 1.9 command will
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            Provider loaded = (await scope.ServiceProvider.GetRequiredService<IProviderRepository>()
                .GetByIdAsync(provider.Id, CancellationToken.None))!;
            loaded.ChangeBookingPolicy(policy, DateTime.UtcNow);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        }

        // Assert
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == provider.Id);
        saved.BookingPolicy.Should().Be(policy);
    }

    [Fact]
    public async Task BookingPolicy_ShouldBeNull_WhenItIsNotSet()
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());

        // Act
        await SaveAsync(provider);

        // Assert
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == provider.Id);
        saved.BookingPolicy.Should().BeNull();
    }

    [Theory]
    [InlineData("None", 10, null, 7)]
    [InlineData("None", null, 3, 7)]
    [InlineData("Percentage", null, null, 7)]
    [InlineData("FixedAmount", 5000, null, 7)]
    public async Task BookingPolicy_ShouldBeRejected_WhenDepositColumnsDoNotMatchType(
        string depositType, int? depositValue, int? depositDueDays, int freeCancellationDays)
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        await SaveAsync(provider);

        // Act
        Func<Task> act = () => UpdateBookingPolicyAsync(
            provider.Id, depositType, depositValue, depositDueDays, freeCancellationDays);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, CheckViolation);
        error.ConstraintName.Should().Be("ck_providers_booking_deposit");
    }

    [Theory]
    [InlineData(null, 7)]
    [InlineData("None", null)]
    public async Task BookingPolicy_ShouldBeRejected_WhenItIsIncomplete(string? depositType, int? freeCancellationDays)
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        await SaveAsync(provider);

        // Act
        Func<Task> act = () => UpdateBookingPolicyAsync(provider.Id, depositType, null, null, freeCancellationDays);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, CheckViolation);
        error.ConstraintName.Should().Be("ck_providers_booking_policy_complete");
    }

    [Theory]
    [InlineData(20)]
    [InlineData(0)]
    [InlineData(null)]
    public async Task VenueDetails_ShouldBeStored_WhenTheyAreSet(int? maxAdults)
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        VenueDetails details = VenueDetails.Create(
            30, maxAdults, 3, 10, OutsideFoodRule.AllowedWithFee, OutsideFoodRule.Allowed, FoodService.Included,
            hasOwnAnimator: true, hasOwnPhotographer: true, hasSmokingArea: false).Value;
        provider.ChangeVenueDetails(details, DateTime.UtcNow);

        // Act
        await SaveAsync(provider);

        // Assert
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == provider.Id);
        saved.VenueDetails.Should().Be(details);
    }

    [Fact]
    public async Task VenueDetails_ShouldBeNull_WhenTheyAreNotSet()
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());

        // Act
        await SaveAsync(provider);

        // Assert
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == provider.Id);
        saved.VenueDetails.Should().BeNull();
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ChangeVenueDetails_ShouldBeStored_WhenProviderIsLoadedFromDatabase(bool hadDetails)
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        if (hadDetails)
        {
            provider.ChangeVenueDetails(
                VenueDetails.Create(
                    20, null, 2, 8, OutsideFoodRule.Allowed, OutsideFoodRule.Allowed, FoodService.None,
                    hasOwnAnimator: false, hasOwnPhotographer: false, hasSmokingArea: true).Value,
                DateTime.UtcNow);
        }

        await SaveAsync(provider);
        VenueDetails details = VenueDetails.Create(
            40, 30, 4, 12, OutsideFoodRule.NotAllowed, OutsideFoodRule.AllowedWithFee, FoodService.ForPurchase,
            hasOwnAnimator: true, hasOwnPhotographer: false, hasSmokingArea: false).Value;

        // Act: load tracked in a new scope and replace the details, as the 1.9 command will
        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            Provider loaded = (await scope.ServiceProvider.GetRequiredService<IProviderRepository>()
                .GetByIdAsync(provider.Id, CancellationToken.None))!;
            loaded.ChangeVenueDetails(details, DateTime.UtcNow);
            await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
        }

        // Assert
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == provider.Id);
        saved.VenueDetails.Should().Be(details);
    }

    [Theory]
    [InlineData(null, 3, "Included", true)]
    [InlineData(30, null, "Included", true)]
    [InlineData(30, 3, null, true)]
    [InlineData(30, 3, "Included", null)]
    public async Task VenueDetails_ShouldBeRejected_WhenTheyAreIncomplete(
        int? maxChildren, int? minAge, string? foodService, bool? hasSmokingArea)
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        await SaveAsync(provider);

        // Act
        Func<Task> act = () => UpdateVenueDetailsAsync(
            provider.Id, maxChildren: maxChildren, minAge: minAge, foodService: foodService, hasSmokingArea: hasSmokingArea);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, CheckViolation);
        error.ConstraintName.Should().Be("ck_providers_venue_details_complete");
    }

    [Fact]
    public async Task VenueDetails_ShouldBeRejected_WhenMinAgeIsGreaterThanMaxAge()
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug());
        await SaveAsync(provider);

        // Act
        Func<Task> act = () => UpdateVenueDetailsAsync(provider.Id, minAge: 7, maxAge: 6);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, CheckViolation);
        error.ConstraintName.Should().Be("ck_providers_venue_age_range");
    }

    [Theory]
    [InlineData(ProviderCategory.Catering)]
    [InlineData(ProviderCategory.Cakes)]
    public async Task VenueDetails_ShouldBeRejected_WhenProviderIsNotAVenue(ProviderCategory category)
    {
        // Arrange
        Provider provider = await CreateInCityWithoutDistrictsAsync(UniqueSlug(), category: category);
        await SaveAsync(provider);

        // Act
        Func<Task> act = () => UpdateVenueDetailsAsync(provider.Id);

        // Assert
        PostgresException error = await ShouldFailInDatabase(act, CheckViolation);
        error.ConstraintName.Should().Be("ck_providers_venue_details_category");
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

    private async Task<Provider> CreateInCityWithoutDistrictsAsync(
        string slug, GeoCoordinate? location = null, ProviderCategory category = ProviderCategory.Venue)
    {
        City city = await DbContext.Cities.AsNoTracking().SingleAsync(c => c.Slug == CityWithoutDistrictsSlug);

        return Provider.Create(
            slug, Faker.Company.CompanyName(), category, null, Faker.Address.StreetAddress(),
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

    // Writes the columns directly, past the domain rules, so only the check constraints can stop it
    private async Task UpdateBookingPolicyAsync(
        Guid id, string? depositType, int? depositValue, int? depositDueDays, int? freeCancellationDays)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        KidsPartyVenueDbContext context = scope.ServiceProvider.GetRequiredService<KidsPartyVenueDbContext>();
        await context.Database.ExecuteSqlAsync(
            $"""
            UPDATE providers
            SET booking_deposit_type = {depositType},
                booking_deposit_value = {depositValue},
                booking_deposit_due_days = {depositDueDays},
                booking_free_cancellation_days = {freeCancellationDays}
            WHERE id = {id}
            """);
    }

    // A complete, valid set by default, written past the domain rules; each test breaks one part of it
    private async Task UpdateVenueDetailsAsync(
        Guid id,
        int? maxChildren = 30,
        int? minAge = 3,
        int? maxAge = 10,
        string? foodService = "Included",
        bool? hasSmokingArea = false)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        KidsPartyVenueDbContext context = scope.ServiceProvider.GetRequiredService<KidsPartyVenueDbContext>();
        await context.Database.ExecuteSqlAsync(
            $"""
            UPDATE providers
            SET venue_max_children = {maxChildren},
                venue_max_adults = {(int?)null},
                venue_min_age = {minAge},
                venue_max_age = {maxAge},
                venue_outside_catering = {"Allowed"},
                venue_outside_cake = {"NotAllowed"},
                venue_food_service = {foodService},
                venue_has_own_animator = {false},
                venue_has_own_photographer = {false},
                venue_has_smoking_area = {hasSmokingArea}
            WHERE id = {id}
            """);
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
