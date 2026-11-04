using System.Collections.Frozen;
using System.Globalization;

namespace KidsPartyVenue.Domain.Providers;

/// <summary>
/// The rules for a provider's slug on top of <see cref="Slugs.UrlSlug"/>: it is unique and never a reserved word.
/// </summary>
public static class ProviderSlug
{
    // A provider's page is /{category}/{city}/{slug}, and district pages are /{category}/{city}/kvart/{district}
    public static readonly FrozenSet<string> Reserved = new[] { "kvart" }.ToFrozenSet();

    public static bool IsReserved(string slug) => Reserved.Contains(slug);

    /// <summary>
    /// Returns the base when it is free, otherwise the base with the lowest free number ("kockica-2").
    /// The suffix is added to the base as it is, so the taken slugs are the base and those starting with "{base}-".
    /// </summary>
    public static string MakeUnique(string baseSlug, IEnumerable<string> takenSlugs)
    {
        HashSet<string> taken = [.. takenSlugs, .. Reserved];
        if (!taken.Contains(baseSlug))
        {
            return baseSlug;
        }

        for (int number = 2; ; number++)
        {
            string candidate = $"{baseSlug}-{number.ToString(CultureInfo.InvariantCulture)}";
            if (!taken.Contains(candidate))
            {
                return candidate;
            }
        }
    }
}
