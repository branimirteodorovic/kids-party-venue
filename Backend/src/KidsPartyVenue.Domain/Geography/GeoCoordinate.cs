namespace KidsPartyVenue.Domain.Geography;

/// <summary>
/// A point on the earth in WGS 84 degrees. Infrastructure stores it as <c>geography(Point, 4326)</c>.
/// </summary>
public sealed record GeoCoordinate
{
    private GeoCoordinate(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }

    public double Longitude { get; }

    public static Result<GeoCoordinate> Create(double latitude, double longitude)
    {
        if (latitude is < -90 or > 90 || longitude is < -180 or > 180 || double.IsNaN(latitude) || double.IsNaN(longitude))
        {
            return Result.Failure<GeoCoordinate>(GeographyErrors.InvalidCoordinate);
        }

        return new GeoCoordinate(latitude, longitude);
    }
}
