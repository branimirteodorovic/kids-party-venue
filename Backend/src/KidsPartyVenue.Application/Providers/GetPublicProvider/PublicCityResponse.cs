namespace KidsPartyVenue.Application.Providers.GetPublicProvider;

public sealed record PublicCityResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;

    /// <summary>The form used after "u" ("u Novom Sadu"), or the name where it isn't set.</summary>
    public string LocativeName { get; init; } = string.Empty;
}
