using AwesomeAssertions;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Lookups;
using KidsPartyVenue.Application.Lookups.GetCatalogs;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Lookups;

public sealed class GetCatalogsTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly ICacheService _cache = factory.Services.GetRequiredService<ICacheService>();

    [Fact]
    public async Task GetCatalogs_ShouldReturnOnlyOpenCities_WithTheirCategoriesAndDistricts()
    {
        // Act: no token, the endpoint is anonymous
        CatalogsResponse catalogs = await HttpClient.GetCatalogsAsync();

        // Assert
        CatalogCityResponse noviSad = catalogs.Cities.Should().ContainSingle().Subject;
        noviSad.Slug.Should().Be("novi-sad");
        noviSad.Categories.Should().Equal(ProviderCategory.Venue);
        noviSad.Center.Latitude.Should().BeApproximately(45.26, 0.01);
        noviSad.Center.Longitude.Should().BeApproximately(19.83, 0.01);
        noviSad.Districts.Should().HaveCount(28);
        catalogs.Amenities.Should().HaveCount(13);
    }

    [Theory]
    [InlineData(null, "Novi Sad", "Novom Sadu", "Centar", "Klima uređaj")]
    [InlineData("en", "Novi Sad", "Novi Sad", "City Centre", "Air conditioning")]
    [InlineData("ru", "Нови-Сад", "Нови-Саде", "Центр", "Кондиционер")]
    [InlineData("de", "Novi Sad", "Novom Sadu", "Centar", "Klima uređaj")]
    public async Task GetCatalogs_ShouldReturnNames_InTheRequestLanguage(
        string? acceptLanguage, string cityName, string locativeName, string districtName, string amenityName)
    {
        // Act
        CatalogsResponse catalogs = await HttpClient.GetCatalogsAsync(acceptLanguage);

        // Assert
        CatalogCityResponse noviSad = catalogs.Cities.Single();
        noviSad.Name.Should().Be(cityName);
        noviSad.LocativeName.Should().Be(locativeName);
        noviSad.Districts.Select(d => d.Name).Should().Contain(districtName);
        catalogs.Amenities.Select(a => a.Name).Should().Contain(amenityName);
    }

    [Fact]
    public async Task GetCatalogs_ShouldSortDistrictsByName_AndAmenitiesBySortOrder()
    {
        // Act
        CatalogsResponse catalogs = await HttpClient.GetCatalogsAsync("sr");

        // Assert
        List<string> districts = [.. catalogs.Cities.Single().Districts.Select(d => d.Name)];
        districts[0].Should().Be("Adice");
        districts.IndexOf("Centar").Should().BeLessThan(districts.IndexOf("Čenej"));
        catalogs.Amenities.Select(a => a.Slug).Should().StartWith(["parking", "klima-uredjaj", "wi-fi"]);
    }

    [Fact]
    public async Task GetCatalogs_ShouldServeCachedNames_UntilTheLookupsTagIsRemoved()
    {
        // Arrange
        await HttpClient.GetCatalogsAsync("sr");
        await DbContext.Database.ExecuteSqlRawAsync("UPDATE amenities SET name_sr = 'Parking (renamed)' WHERE slug = 'parking'");

        try
        {
            // Act
            CatalogsResponse cached = await HttpClient.GetCatalogsAsync("sr");
            await _cache.RemoveByTagAsync(LookupsCache.Tag);
            CatalogsResponse fresh = await HttpClient.GetCatalogsAsync("sr");

            // Assert
            cached.Amenities.Single(a => a.Slug == "parking").Name.Should().Be("Parking");
            fresh.Amenities.Single(a => a.Slug == "parking").Name.Should().Be("Parking (renamed)");
        }
        finally
        {
            await DbContext.Database.ExecuteSqlRawAsync("UPDATE amenities SET name_sr = 'Parking' WHERE slug = 'parking'");
            await _cache.RemoveByTagAsync(LookupsCache.Tag);
        }
    }
}
