namespace KidsPartyVenue.Application.Lookups.GetCities;

public sealed record CityResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public List<CityDistrictResponse> Districts { get; init; } = [];
}
