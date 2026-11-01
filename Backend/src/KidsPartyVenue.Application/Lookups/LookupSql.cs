namespace KidsPartyVenue.Application.Lookups;

/// <summary>SQL fragments for lookup names, which are stored as <c>{prefix}_sr</c>, <c>{prefix}_en</c> and <c>{prefix}_ru</c>.</summary>
internal static class LookupSql
{
    /// <summary>The <c>name</c> in the language of the <c>@Language</c> parameter, falling back to Serbian.</summary>
    public const string Name = "COALESCE(CASE @Language WHEN 'en' THEN name_en WHEN 'ru' THEN name_ru END, name_sr)";

    /// <summary>The <c>locative_name</c> in the language of <c>@Language</c>, falling back to Serbian.</summary>
    public const string LocativeName =
        "COALESCE(CASE @Language WHEN 'en' THEN locative_name_en WHEN 'ru' THEN locative_name_ru END, locative_name_sr)";

    /// <summary>
    /// Orders by a <c>name</c> column with the collation of <c>@Language</c> (Serbian puts "Č" after "C" and "Lj" after "L").
    /// Collations can't be parameters, so each language gets its own key and only one of them is non-null.
    /// </summary>
    public const string OrderByName = """
        ORDER BY
            CASE WHEN @Language = 'sr' THEN name END COLLATE "sr-Latn-x-icu",
            CASE WHEN @Language = 'en' THEN name END COLLATE "en-x-icu",
            CASE WHEN @Language = 'ru' THEN name END COLLATE "ru-x-icu"
        """;
}
