using KidsPartyVenue.Application.Abstractions.Localization;
using KidsPartyVenue.Domain.Localization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;

namespace KidsPartyVenue.Infrastructure.Localization;

/// <summary>Reads the language the request localization middleware resolved from <c>Accept-Language</c>.</summary>
internal sealed class LanguageContext(IHttpContextAccessor httpContextAccessor) : ILanguageContext
{
    public Language Current
    {
        get
        {
            string? uiCulture = httpContextAccessor.HttpContext?
                .Features.Get<IRequestCultureFeature>()?
                .RequestCulture.UICulture.Name;

            return LanguageCodes.TryFromCode(uiCulture, out Language language) ? language : LanguageCodes.Default;
        }
    }
}
