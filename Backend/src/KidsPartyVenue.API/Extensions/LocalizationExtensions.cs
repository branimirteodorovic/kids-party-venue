using System.Globalization;
using KidsPartyVenue.Domain.Localization;
using Microsoft.AspNetCore.Localization;

namespace KidsPartyVenue.API.Extensions;

internal static class LocalizationExtensions
{
    /// <summary>
    /// Resolves the request language from <c>Accept-Language</c> only. Just the UI culture follows it;
    /// <see cref="CultureInfo.CurrentCulture"/> stays invariant, so formatting never depends on the caller.
    /// </summary>
    public static IServiceCollection AddRequestLanguage(this IServiceCollection services)
    {
        services.Configure<RequestLocalizationOptions>(options =>
        {
            options.DefaultRequestCulture = new RequestCulture(
                CultureInfo.InvariantCulture,
                new CultureInfo(LanguageCodes.Default.ToCode()));
            options.SupportedCultures = [CultureInfo.InvariantCulture];
            options.SupportedUICultures = [.. LanguageCodes.All.Select(language => new CultureInfo(language.ToCode()))];
            options.RequestCultureProviders = [new AcceptLanguageHeaderRequestCultureProvider()];
            options.ApplyCurrentCultureToResponseHeaders = true;
        });

        return services;
    }
}
