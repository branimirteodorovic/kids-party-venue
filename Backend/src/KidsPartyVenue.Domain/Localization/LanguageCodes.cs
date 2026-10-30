namespace KidsPartyVenue.Domain.Localization;

/// <summary>
/// Maps <see cref="Language"/> to the two-letter codes used in headers, JSON and the database.
/// </summary>
public static class LanguageCodes
{
    public const Language Default = Language.Sr;

    public static IReadOnlyList<Language> All { get; } = [Language.Sr, Language.En, Language.Ru];

    public static string ToCode(this Language language) =>
        language switch
        {
            Language.Sr => "sr",
            Language.En => "en",
            Language.Ru => "ru",
            _ => throw new ArgumentOutOfRangeException(nameof(language), language, "Unsupported language")
        };

    public static bool TryFromCode(string? code, out Language language)
    {
        foreach (Language candidate in All)
        {
            if (string.Equals(candidate.ToCode(), code, StringComparison.OrdinalIgnoreCase))
            {
                language = candidate;
                return true;
            }
        }

        language = Default;
        return false;
    }

    /// <summary>For codes that were validated when stored, such as database values.</summary>
    public static Language FromCode(string code) =>
        TryFromCode(code, out Language language)
            ? language
            : throw new ArgumentException($"Unsupported language code '{code}'", nameof(code));
}
