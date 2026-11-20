namespace KidsPartyVenue.Application.Providers;

/// <summary>
/// Cached public provider pages. A handler that changes a provider calls <c>RemoveByTagAsync(Tag(providerId))</c> after
/// saving; the pages also carry the lookups tag, because they hold city, district and amenity names.
/// </summary>
public static class ProviderCache
{
    /// <summary>A fallback in case an invalidation is missed; writes remove the tag right away.</summary>
    public static readonly TimeSpan Expiration = TimeSpan.FromHours(1);

    public static string Tag(Guid providerId) => $"provider:{providerId}";

    /// <summary>The provider ID of a slug. Slugs never change or move to another provider, so it has no tag.</summary>
    public static string SlugKey(string slug) => $"provider-slugs:{slug}";

    public static string PageKey(Guid providerId, string languageCode) => $"provider-page:{providerId}:{languageCode}";
}
