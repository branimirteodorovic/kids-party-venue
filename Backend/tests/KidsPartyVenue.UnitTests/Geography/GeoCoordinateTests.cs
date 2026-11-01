using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Geography;

namespace KidsPartyVenue.UnitTests.Geography;

public sealed class GeoCoordinateTests
{
    [Theory]
    [InlineData(45.26444, 19.83167)]
    [InlineData(-90, -180)]
    [InlineData(90, 180)]
    public void Create_ShouldReturnCoordinate_WhenItIsInRange(double latitude, double longitude)
    {
        // Act
        Result<GeoCoordinate> result = GeoCoordinate.Create(latitude, longitude);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Latitude.Should().Be(latitude);
        result.Value.Longitude.Should().Be(longitude);
    }

    [Theory]
    [InlineData(90.1, 0)]
    [InlineData(-90.1, 0)]
    [InlineData(0, 180.1)]
    [InlineData(0, -180.1)]
    [InlineData(double.NaN, 0)]
    [InlineData(0, double.NaN)]
    public void Create_ShouldFail_WhenItIsOutOfRange(double latitude, double longitude)
    {
        // Act
        Result<GeoCoordinate> result = GeoCoordinate.Create(latitude, longitude);

        // Assert
        result.Error.Should().Be(GeographyErrors.InvalidCoordinate);
    }
}
