namespace KidsPartyVenue.Application.Lookups.GetCatalogs;

public sealed record CoordinateResponse
{
    public double Latitude { get; init; }
    public double Longitude { get; init; }
}
