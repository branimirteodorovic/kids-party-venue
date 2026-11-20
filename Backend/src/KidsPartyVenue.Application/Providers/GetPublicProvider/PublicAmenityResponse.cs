namespace KidsPartyVenue.Application.Providers.GetPublicProvider;

public sealed record PublicAmenityResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
