using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Users.Login;

public sealed record LoginUserCommand(string Email, string Password) : ICommand<AuthTokens>;
