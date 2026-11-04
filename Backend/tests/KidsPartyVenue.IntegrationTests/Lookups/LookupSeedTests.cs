using AwesomeAssertions;
using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Slugs;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.IntegrationTests.Lookups;

public sealed class LookupSeedTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    // 145 units outside Kosovo and Metohija and 29 in it, as the Law on Territorial Organization lists them
    private const int RegisteredUnits = 174;

    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task Migration_ShouldSeedEveryCity_WithACenterInsideSerbia()
    {
        // Act
        List<City> cities = await DbContext.Cities.AsNoTracking().ToListAsync();

        // Assert
        cities.Should().HaveCount(RegisteredUnits);
        cities.Select(c => c.Slug).Should().OnlyHaveUniqueItems();
        cities.Should().AllSatisfy(c =>
        {
            c.Name.Sr.Should().NotBeNullOrWhiteSpace();
            c.Center.Latitude.Should().BeInRange(41.8, 46.2);
            c.Center.Longitude.Should().BeInRange(18.8, 23.1);
        });
        cities.Select(c => c.Slug).Should().Contain(["novi-sad", "beograd", "nis", "pristina", "kosovska-mitrovica"]);
        cities.Single(c => c.Slug == "beograd").Name.En.Should().Be("Belgrade");
    }

    [Fact]
    public async Task Migration_ShouldSeedLocativeNames_ForNoviSadOnly()
    {
        // Act
        List<City> withLocative = await DbContext.Cities.AsNoTracking().Where(c => c.LocativeName != null).ToListAsync();

        // Assert
        City noviSad = withLocative.Should().ContainSingle().Subject;
        noviSad.Slug.Should().Be(DevelopmentSeeder.NoviSadSlug);
        noviSad.LocativeName!.Sr.Should().Be("Novom Sadu");
        noviSad.LocativeName.En.Should().Be("Novi Sad");
        noviSad.LocativeName.Ru.Should().Be("Нови-Саде");
    }

    [Fact]
    public async Task Migration_ShouldSeedNoviSadDistrictsAndAmenities()
    {
        // Arrange
        City noviSad = await DbContext.Cities.AsNoTracking().SingleAsync(c => c.Slug == DevelopmentSeeder.NoviSadSlug);

        // Act
        List<District> districts = await DbContext.Districts.AsNoTracking().ToListAsync();
        List<Amenity> amenities = await DbContext.Amenities.AsNoTracking().OrderBy(a => a.SortOrder).ToListAsync();

        // Assert
        districts.Should().HaveCount(28).And.OnlyContain(d => d.CityId == noviSad.Id);
        districts.Select(d => d.Slug).Should().Contain(["liman", "petrovaradin", "avijaticarsko-naselje"]);
        amenities.Should().HaveCount(13);
        amenities.Select(a => a.Slug).Should().StartWith(["parking", "klima-uredjaj", "wi-fi"]);
    }

    [Fact]
    public async Task FromName_ShouldGiveSeededSlug_ForEverySeededSerbianName()
    {
        // Act
        List<City> cities = await DbContext.Cities.AsNoTracking().ToListAsync();
        List<District> districts = await DbContext.Districts.AsNoTracking().ToListAsync();
        List<Amenity> amenities = await DbContext.Amenities.AsNoTracking().ToListAsync();

        // Assert
        cities.Should().AllSatisfy(c => UrlSlug.FromName(c.Name.Sr).Should().Be(c.Slug));
        districts.Should().AllSatisfy(d => UrlSlug.FromName(d.Name.Sr).Should().Be(d.Slug));
        amenities.Should().AllSatisfy(a => UrlSlug.FromName(a.Name.Sr).Should().Be(a.Slug));
    }

    [Fact]
    public async Task SeedDevelopmentData_ShouldOpenVenuesInNoviSadOnce_WhenItRunsAgain()
    {
        // Act: it already ran when the test host started
        await new ApplicationBuilder(_factory.Services).SeedDevelopmentDataAsync();

        // Assert
        List<City> open = await DbContext.Cities.AsNoTracking()
            .Include(c => c.Categories)
            .Where(c => c.Categories.Any())
            .ToListAsync();
        City noviSad = open.Should().ContainSingle().Subject;
        noviSad.Slug.Should().Be(DevelopmentSeeder.NoviSadSlug);
        noviSad.Categories.Should().ContainSingle().Which.Category.Should().Be(ProviderCategory.Venue);
    }
}
