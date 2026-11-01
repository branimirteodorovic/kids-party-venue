using System.Text.RegularExpressions;

namespace KidsPartyVenue.Domain.Lookups;

/// <summary>
/// The URL form of a lookup name: lowercase ASCII letters and digits separated by single hyphens ("novi-sad").
/// </summary>
public static partial class LookupSlug
{
    public const int MaxLength = 100;

    public static bool IsValid(string? slug) =>
        slug is { Length: > 0 and <= MaxLength } && SlugRegex().IsMatch(slug);

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
