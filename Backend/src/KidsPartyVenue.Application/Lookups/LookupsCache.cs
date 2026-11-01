namespace KidsPartyVenue.Application.Lookups;

/// <summary>
/// Cached lookup responses (cities, districts, amenities, open categories). Every lookup write affects all of them,
/// so they share one tag without an ID.
/// </summary>
public static class LookupsCache
{
    public const string Tag = "lookups";

    public static readonly TimeSpan Expiration = TimeSpan.FromHours(1);
}
