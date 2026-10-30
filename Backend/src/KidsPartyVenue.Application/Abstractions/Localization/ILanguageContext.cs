using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.Application.Abstractions.Localization;

public interface ILanguageContext
{
    /// <summary>The language of the current request, from <c>Accept-Language</c>. <c>sr</c> outside a request.</summary>
    Language Current { get; }
}
