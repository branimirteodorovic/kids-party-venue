namespace KidsPartyVenue.Domain.Localization;

public static class LocalizationErrors
{
    public static readonly Error SerbianTextRequired = Error.Validation("Localization.SerbianTextRequired", "Serbian text is required");
    public static readonly Error UnsupportedLanguage = Error.Validation("Localization.UnsupportedLanguage", "Language is not supported");
}
