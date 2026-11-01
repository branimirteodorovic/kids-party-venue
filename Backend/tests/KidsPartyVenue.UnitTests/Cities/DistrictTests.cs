using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.UnitTests.Cities;

public sealed class DistrictTests
{
    private static readonly LocalizedText Name = LocalizedText.Create("Liman", ru: "Лиман").Value;

    [Fact]
    public void Create_ShouldReturnDistrict_WhenInputIsValid()
    {
        // Arrange
        var cityId = Guid.CreateVersion7();

        // Act
        Result<District> result = District.Create(cityId, "liman", Name);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.CityId.Should().Be(cityId);
        result.Value.Slug.Should().Be("liman");
        result.Value.Name.Should().Be(Name);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Liman")]
    [InlineData("novo naselje")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenSlugIsInvalid(string? slug)
    {
        // Act
        Result<District> result = District.Create(Guid.CreateVersion7(), slug!, Name);

        // Assert
        result.Error.Should().Be(DistrictErrors.InvalidSlug);
    }
}
