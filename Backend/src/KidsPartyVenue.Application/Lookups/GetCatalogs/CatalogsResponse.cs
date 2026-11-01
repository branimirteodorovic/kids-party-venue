namespace KidsPartyVenue.Application.Lookups.GetCatalogs;

public sealed record CatalogsResponse
{
    public List<CatalogCityResponse> Cities { get; init; } = [];
    public List<CatalogAmenityResponse> Amenities { get; init; } = [];
}
