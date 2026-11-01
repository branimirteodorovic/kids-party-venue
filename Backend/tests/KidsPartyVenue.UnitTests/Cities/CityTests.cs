using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.UnitTests.Cities;

public sealed class CityTests
{
    private static readonly LocalizedText Name = LocalizedText.Create("Novi Sad", ru: "Нови-Сад").Value;
    private static readonly LocalizedText Locative = LocalizedText.Create("Novom Sadu", "Novi Sad", "Нови-Саде").Value;
    private static readonly GeoCoordinate Center = GeoCoordinate.Create(45.26444, 19.83167).Value;

    [Fact]
    public void Create_ShouldReturnCity_WhenInputIsValid()
    {
        // Act
        Result<City> result = City.Create("novi-sad", Name, Center, Locative);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Slug.Should().Be("novi-sad");
        result.Value.Name.Should().Be(Name);
        result.Value.LocativeName.Should().Be(Locative);
        result.Value.Center.Should().Be(Center);
        result.Value.Categories.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Novi-Sad")]
    [InlineData("novi sad")]
    [InlineData("novi--sad")]
    [InlineData("-novi-sad")]
    [InlineData("čačak")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenSlugIsInvalid(string? slug)
    {
        // Act
        Result<City> result = City.Create(slug!, Name, Center);

        // Assert
        result.Error.Should().Be(CityErrors.InvalidSlug);
    }

    [Fact]
    public void Create_ShouldFail_WhenSlugIsTooLong()
    {
        // Act
        Result<City> result = City.Create(new string('a', 101), Name, Center);

        // Assert
        result.Error.Should().Be(CityErrors.InvalidSlug);
    }

    [Fact]
    public void OpenCategory_ShouldOpenIt_WhenLocativeNameIsComplete()
    {
        // Arrange
        City city = City.Create("novi-sad", Name, Center, Locative).Value;
        DateTime utcNow = DateTime.UtcNow;

        // Act
        Result result = city.OpenCategory(ProviderCategory.Venue, utcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        CityCategory category = city.Categories.Should().ContainSingle().Subject;
        category.CityId.Should().Be(city.Id);
        category.Category.Should().Be(ProviderCategory.Venue);
        category.OpenedAtUtc.Should().Be(utcNow);
    }

    [Fact]
    public void OpenCategory_ShouldFail_WhenLocativeNameIsMissing()
    {
        // Arrange
        City city = City.Create("novi-sad", Name, Center).Value;

        // Act
        Result result = city.OpenCategory(ProviderCategory.Venue, DateTime.UtcNow);

        // Assert
        result.Error.Should().Be(CityErrors.LocativeNameRequired);
        city.Categories.Should().BeEmpty();
    }

    [Theory]
    [InlineData(null, "Нови-Саде")]
    [InlineData("Novi Sad", null)]
    public void OpenCategory_ShouldFail_WhenLocativeNameIsMissingALanguage(string? en, string? ru)
    {
        // Arrange
        LocalizedText locative = LocalizedText.Create("Novom Sadu", en, ru).Value;
        City city = City.Create("novi-sad", Name, Center, locative).Value;

        // Act
        Result result = city.OpenCategory(ProviderCategory.Venue, DateTime.UtcNow);

        // Assert
        result.Error.Should().Be(CityErrors.LocativeNameRequired);
    }

    [Fact]
    public void OpenCategory_ShouldFail_WhenItIsAlreadyOpen()
    {
        // Arrange
        City city = City.Create("novi-sad", Name, Center, Locative).Value;
        city.OpenCategory(ProviderCategory.Venue, DateTime.UtcNow);

        // Act
        Result result = city.OpenCategory(ProviderCategory.Venue, DateTime.UtcNow);

        // Assert
        result.Error.Should().Be(CityErrors.CategoryAlreadyOpen);
        city.Categories.Should().ContainSingle();
    }

    [Fact]
    public void OpenCategory_ShouldFail_WhenCategoryIsUnknown()
    {
        // Arrange
        City city = City.Create("novi-sad", Name, Center, Locative).Value;

        // Act
        Result result = city.OpenCategory((ProviderCategory)99, DateTime.UtcNow);

        // Assert
        result.Error.Should().Be(CityErrors.UnknownCategory);
    }

    [Fact]
    public void CloseCategory_ShouldCloseIt_WhenItIsOpen()
    {
        // Arrange
        City city = City.Create("novi-sad", Name, Center, Locative).Value;
        city.OpenCategory(ProviderCategory.Venue, DateTime.UtcNow);
        city.OpenCategory(ProviderCategory.Cakes, DateTime.UtcNow);

        // Act
        Result result = city.CloseCategory(ProviderCategory.Venue);

        // Assert
        result.IsSuccess.Should().BeTrue();
        city.Categories.Should().ContainSingle(c => c.Category == ProviderCategory.Cakes);
    }

    [Fact]
    public void CloseCategory_ShouldFail_WhenItIsNotOpen()
    {
        // Arrange
        City city = City.Create("novi-sad", Name, Center, Locative).Value;

        // Act
        Result result = city.CloseCategory(ProviderCategory.Venue);

        // Assert
        result.Error.Should().Be(CityErrors.CategoryNotOpen);
    }
}
