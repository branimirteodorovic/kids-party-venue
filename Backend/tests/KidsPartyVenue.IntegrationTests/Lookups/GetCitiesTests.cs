using AwesomeAssertions;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Lookups;
using KidsPartyVenue.Application.Lookups.GetCities;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Lookups;

public sealed class GetCitiesTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly ICacheService _cache = factory.Services.GetRequiredService<ICacheService>();

    [Fact]
    public async Task GetCities_ShouldReturnEveryCity_OpenOrNot()
    {
        // Act: no token, the endpoint is anonymous
        List<CityResponse> cities = await HttpClient.GetCitiesAsync();

        // Assert
        cities.Should().HaveCount(174);
        cities.Single(c => c.Slug == "novi-sad").Districts.Should().HaveCount(28);
        cities.Single(c => c.Slug == "beograd").Districts.Should().BeEmpty();
    }

    [Theory]
    // Kragujevac has no English name, so English falls back to Serbian
    [InlineData("sr", "Beograd", "Kragujevac")]
    [InlineData("en", "Belgrade", "Kragujevac")]
    [InlineData("ru", "Белград", "Крагуевац")]
    public async Task GetCities_ShouldReturnNames_InTheRequestLanguage_FallingBackToSerbian(
        string acceptLanguage, string belgrade, string kragujevac)
    {
        // Act
        List<CityResponse> cities = await HttpClient.GetCitiesAsync(acceptLanguage);

        // Assert
        cities.Single(c => c.Slug == "beograd").Name.Should().Be(belgrade);
        cities.Single(c => c.Slug == "kragujevac").Name.Should().Be(kragujevac);
    }

    [Fact]
    public async Task GetCities_ShouldSortByName_WithSerbianCollation()
    {
        // Act
        List<CityResponse> cities = await HttpClient.GetCitiesAsync("sr");

        // Assert: Č and Lj are letters of their own in Serbian, after C and L
        List<string> names = [.. cities.Select(c => c.Name)];
        names.IndexOf("Crna Trava").Should().BeLessThan(names.IndexOf("Čačak"));
        names.IndexOf("Lučani").Should().BeLessThan(names.IndexOf("Ljig"));
        names[0].Should().Be("Ada");
    }

    [Fact]
    public async Task GetCities_ShouldFallBackToSerbian_WhenRussianNameIsMissing()
    {
        // Arrange: a district with only a Serbian name, in a city with none
        Guid districtId = Guid.CreateVersion7();
        await DbContext.Database.ExecuteSqlInterpolatedAsync(
            $"INSERT INTO districts (id, city_id, slug, name_sr) SELECT {districtId}, id, 'zemun', 'Zemun' FROM cities WHERE slug = 'beograd'");
        await _cache.RemoveByTagAsync(LookupsCache.Tag);

        try
        {
            // Act
            List<CityResponse> cities = await HttpClient.GetCitiesAsync("ru");

            // Assert
            cities.Single(c => c.Slug == "beograd").Districts.Should().ContainSingle().Which.Name.Should().Be("Zemun");
        }
        finally
        {
            await DbContext.Database.ExecuteSqlInterpolatedAsync($"DELETE FROM districts WHERE id = {districtId}");
            await _cache.RemoveByTagAsync(LookupsCache.Tag);
        }
    }

    [Fact]
    public async Task GetCities_ShouldServeCachedNames_UntilTheLookupsTagIsRemoved()
    {
        // Arrange
        await HttpClient.GetCitiesAsync("sr");
        await DbContext.Database.ExecuteSqlRawAsync("UPDATE cities SET name_sr = 'Ada (renamed)' WHERE slug = 'ada'");

        try
        {
            // Act
            List<CityResponse> cached = await HttpClient.GetCitiesAsync("sr");
            await _cache.RemoveByTagAsync(LookupsCache.Tag);
            List<CityResponse> fresh = await HttpClient.GetCitiesAsync("sr");

            // Assert
            cached.Single(c => c.Slug == "ada").Name.Should().Be("Ada");
            fresh.Single(c => c.Slug == "ada").Name.Should().Be("Ada (renamed)");
        }
        finally
        {
            await DbContext.Database.ExecuteSqlRawAsync("UPDATE cities SET name_sr = 'Ada' WHERE slug = 'ada'");
            await _cache.RemoveByTagAsync(LookupsCache.Tag);
        }
    }
}
