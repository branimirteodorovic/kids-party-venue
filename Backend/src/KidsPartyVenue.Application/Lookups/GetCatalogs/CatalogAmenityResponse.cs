namespace KidsPartyVenue.Application.Lookups.GetCatalogs;

public sealed record CatalogAmenityResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
