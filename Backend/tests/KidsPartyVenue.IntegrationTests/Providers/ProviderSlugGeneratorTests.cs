using AwesomeAssertions;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class ProviderSlugGeneratorTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    // Beograd has no districts in the seed, so a provider there needs none
    private const string CityWithoutDistrictsSlug = "beograd";

    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task GenerateAsync_ShouldReturnBase_WhenNoProviderHasIt()
    {
        // Arrange: the tests share one database, so each one gets its own name
        (string name, string baseSlug) = UniqueName();

        // Act
        string slug = await GenerateAsync(name);

        // Assert
        slug.Should().Be(baseSlug);
    }

    [Fact]
    public async Task GenerateAsync_ShouldAddTwo_WhenBaseIsTaken()
    {
        // Arrange
        (string name, string baseSlug) = UniqueName();
        await SaveWithSlugsAsync(baseSlug);

        // Act
        string slug = await GenerateAsync(name);

        // Assert
        slug.Should().Be($"{baseSlug}-2");
    }

    [Fact]
    public async Task GenerateAsync_ShouldAddThree_WhenBaseAndTwoAreTaken()
    {
        // Arrange
        (string name, string baseSlug) = UniqueName();
        await SaveWithSlugsAsync(baseSlug, $"{baseSlug}-2");

        // Act
        string slug = await GenerateAsync(name);

        // Assert
        slug.Should().Be($"{baseSlug}-3");
    }

    [Fact]
    public async Task GenerateAsync_ShouldReturnBase_WhenOnlyLongerSlugsStartWithIt()
    {
        // Arrange
        (string name, string baseSlug) = UniqueName();
        await SaveWithSlugsAsync($"{baseSlug}-bar", $"{baseSlug}a");

        // Act
        string slug = await GenerateAsync(name);

        // Assert
        slug.Should().Be(baseSlug);
    }

    private static (string Name, string BaseSlug) UniqueName()
    {
        string suffix = Guid.CreateVersion7().ToString("N");

        return ($"Kockica {suffix}", $"kockica-{suffix}");
    }

    private async Task<string> GenerateAsync(string name)
    {
        using IServiceScope scope = _factory.Services.CreateScope();

        return await scope.ServiceProvider.GetRequiredService<IProviderSlugGenerator>()
            .GenerateAsync(name, CancellationToken.None);
    }

    private async Task SaveWithSlugsAsync(params string[] slugs)
    {
        City city = await DbContext.Cities.AsNoTracking().SingleAsync(c => c.Slug == CityWithoutDistrictsSlug);

        using IServiceScope scope = _factory.Services.CreateScope();
        IProviderRepository repository = scope.ServiceProvider.GetRequiredService<IProviderRepository>();
        foreach (string slug in slugs)
        {
            repository.Insert(Provider.Create(
                slug, Faker.Company.CompanyName(), ProviderCategory.Venue, null, Faker.Address.StreetAddress(),
                city.Id, null, cityHasDistricts: false, GeoCoordinate.Create(44.8125, 20.4612).Value,
                null, isClaimed: false, DateTime.UtcNow).Value);
        }

        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync();
    }
}
