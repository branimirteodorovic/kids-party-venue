namespace KidsPartyVenue.Domain.Geography;

public static class GeographyErrors
{
    public static readonly Error InvalidCoordinate = Error.Validation("Geography.InvalidCoordinate", "Latitude must be within ±90 and longitude within ±180 degrees");
}
