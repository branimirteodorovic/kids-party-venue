using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.Application.Users.ChangePreferredLanguage;

public sealed record ChangePreferredLanguageCommand(Language Language) : ICommand;
