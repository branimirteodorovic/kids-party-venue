namespace KidsPartyVenue.Application.Providers;

/// <summary>The provider's new row version after an edit, which the editor sends with its next change.</summary>
public sealed record ProviderVersionResponse(uint Version);
