namespace KidsPartyVenue.Application.Providers;

/// <summary>SQL fragments shared by provider queries.</summary>
internal static class ProviderSql
{
    /// <summary>
    /// The one rule for what parents may see, against the alias <c>p</c> for <c>providers</c>: published, not suspended,
    /// and its category open in its city. Every public query (page, search, free slots, inquiries) uses it.
    /// </summary>
    public const string PubliclyVisible = """
        (p.status = 'Published'
            AND p.suspended_at_utc IS NULL
            AND EXISTS (SELECT 1 FROM city_categories cc WHERE cc.city_id = p.city_id AND cc.category = p.category))
        """;
}
