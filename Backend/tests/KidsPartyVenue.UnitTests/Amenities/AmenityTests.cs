using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.UnitTests.Amenities;

public sealed class AmenityTests
{
    private static readonly LocalizedText Name = LocalizedText.Create("Parking", "Parking", "Парковка").Value;

    [Fact]
    public void Create_ShouldReturnAmenity_WhenInputIsValid()
    {
        // Act
        Result<Amenity> result = Amenity.Create("parking", Name, 3);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Id.Should().NotBeEmpty();
        result.Value.Slug.Should().Be("parking");
        result.Value.Name.Should().Be(Name);
        result.Value.SortOrder.Should().Be(3);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Parking")]
    [InlineData("wi_fi")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenSlugIsInvalid(string? slug)
    {
        // Act
        Result<Amenity> result = Amenity.Create(slug!, Name, 1);

        // Assert
        result.Error.Should().Be(AmenityErrors.InvalidSlug);
    }
}
