using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.Application.Users.Register;

/// <param name="PreferredLanguage">When null, the request language is used.</param>
public sealed record RegisterUserCommand(string Email, string Password, string FullName, Language? PreferredLanguage = null)
    : ICommand<Guid>;
