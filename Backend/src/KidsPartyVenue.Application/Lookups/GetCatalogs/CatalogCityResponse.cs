using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Lookups.GetCatalogs;

public sealed record CatalogCityResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    /// <summary>The form used after "u" ("u Novom Sadu"); in English, the name itself.</summary>
    public string LocativeName { get; init; } = string.Empty;
    public CoordinateResponse Center { get; init; } = new();
    public List<ProviderCategory> Categories { get; init; } = [];
    public List<CatalogDistrictResponse> Districts { get; init; } = [];
}
