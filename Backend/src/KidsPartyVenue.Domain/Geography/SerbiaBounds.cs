namespace KidsPartyVenue.Domain.Geography;

/// <summary>The bounding box of Serbia, Kosovo and Metohija included, with a small margin.</summary>
public static class SerbiaBounds
{
    public const double MinLatitude = 41.8;
    public const double MaxLatitude = 46.2;
    public const double MinLongitude = 18.8;
    public const double MaxLongitude = 23.1;

    public static bool Contains(GeoCoordinate coordinate) =>
        coordinate.Latitude is >= MinLatitude and <= MaxLatitude &&
        coordinate.Longitude is >= MinLongitude and <= MaxLongitude;
}
