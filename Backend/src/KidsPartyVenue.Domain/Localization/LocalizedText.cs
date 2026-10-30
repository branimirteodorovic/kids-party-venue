namespace KidsPartyVenue.Domain.Localization;

/// <summary>
/// Lookup text in every supported language. Serbian is required; a missing English or Russian
/// translation falls back to Serbian.
/// </summary>
public sealed record LocalizedText
{
    private LocalizedText(string sr, string? en, string? ru)
    {
        Sr = sr;
        En = en;
        Ru = ru;
    }

    public string Sr { get; }

    public string? En { get; }

    public string? Ru { get; }

    public static Result<LocalizedText> Create(string sr, string? en = null, string? ru = null)
    {
        if (string.IsNullOrWhiteSpace(sr))
        {
            return Result.Failure<LocalizedText>(LocalizationErrors.SerbianTextRequired);
        }

        return new LocalizedText(sr.Trim(), Normalize(en), Normalize(ru));
    }

    public string Get(Language language) =>
        language switch
        {
            Language.En => En ?? Sr,
            Language.Ru => Ru ?? Sr,
            _ => Sr
        };

    private static string? Normalize(string? text) => string.IsNullOrWhiteSpace(text) ? null : text.Trim();
}
