using System.Collections.Frozen;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace KidsPartyVenue.Domain.Slugs;

/// <summary>
/// The URL form of a name: lowercase ASCII letters and digits separated by single hyphens ("novi-sad").
/// </summary>
public static partial class UrlSlug
{
    public const int MaxLength = 100;

    /// <summary>
    /// The length <see cref="FromName"/> cuts to, which keeps generated URLs readable.
    /// </summary>
    public const int GeneratedMaxLength = 60;

    /// <summary>
    /// The slug of a name that has no letters or digits to keep.
    /// </summary>
    public const string Fallback = "profil";

    // Serbian Cyrillic, plus the Russian letters Serbian doesn't have. Latin č, ć, š and ž lose their marks under NFKD; đ doesn't decompose
    private static readonly FrozenDictionary<char, string> Transliterations = new Dictionary<char, string>
    {
        ['а'] = "a", ['б'] = "b", ['в'] = "v", ['г'] = "g", ['д'] = "d", ['ђ'] = "dj", ['е'] = "e", ['ж'] = "z",
        ['з'] = "z", ['и'] = "i", ['ј'] = "j", ['к'] = "k", ['л'] = "l", ['љ'] = "lj", ['м'] = "m", ['н'] = "n",
        ['њ'] = "nj", ['о'] = "o", ['п'] = "p", ['р'] = "r", ['с'] = "s", ['т'] = "t", ['ћ'] = "c", ['у'] = "u",
        ['ф'] = "f", ['х'] = "h", ['ц'] = "c", ['ч'] = "c", ['џ'] = "dz", ['ш'] = "s",
        ['й'] = "j", ['я'] = "ja", ['ю'] = "ju", ['ё'] = "jo", ['ы'] = "i", ['э'] = "e", ['щ'] = "sc", ['ъ'] = "", ['ь'] = "",
        ['đ'] = "dj"
    }.ToFrozenDictionary();

    public static bool IsValid(string? slug) =>
        slug is { Length: > 0 and <= MaxLength } && SlugRegex().IsMatch(slug);

    /// <summary>
    /// Transliterates a name to its slug ("Рођенданска Журка" and "Rođendanska Žurka" give "rodjendanska-zurka"),
    /// at most <see cref="GeneratedMaxLength"/> characters, or <see cref="Fallback"/> when nothing is left.
    /// </summary>
    public static string FromName(string? name)
    {
        StringBuilder slug = new();
        bool hyphenPending = false;

        foreach (char original in (name ?? string.Empty).ToLowerInvariant())
        {
            string letters = Transliterations.TryGetValue(original, out string? latin) ? latin : original.ToString();

            foreach (char c in letters.Normalize(NormalizationForm.FormKD))
            {
                if (c is (>= 'a' and <= 'z') or (>= '0' and <= '9'))
                {
                    if (hyphenPending && slug.Length > 0)
                    {
                        slug.Append('-');
                    }

                    hyphenPending = false;
                    slug.Append(c);
                }
                else if (!IsDropped(c))
                {
                    hyphenPending = true;
                }
            }
        }

        string shortened = Shorten(slug.ToString(), GeneratedMaxLength);

        return shortened.Length == 0 ? Fallback : shortened;
    }

    /// <summary>
    /// Cuts a valid slug to at most <paramref name="maxLength"/> characters, at the last hyphen when there is one.
    /// </summary>
    public static string Shorten(string slug, int maxLength)
    {
        if (slug.Length <= maxLength)
        {
            return slug;
        }

        // The character at maxLength is the first one cut off, so a hyphen there still keeps whole words
        int lastHyphen = slug.LastIndexOf('-', maxLength);

        return lastHyphen > 0 ? slug[..lastHyphen] : slug[..maxLength];
    }

    // Apostrophes join a word ("Mike's" is "mikes"); combining marks are the diacritics NFKD split off
    private static bool IsDropped(char c) =>
        c is '\'' or '\u2019' || char.GetUnicodeCategory(c) == UnicodeCategory.NonSpacingMark;

    [GeneratedRegex("^[a-z0-9]+(-[a-z0-9]+)*$")]
    private static partial Regex SlugRegex();
}
