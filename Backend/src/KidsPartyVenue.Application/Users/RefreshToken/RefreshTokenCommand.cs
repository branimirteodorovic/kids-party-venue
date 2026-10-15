using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Users.RefreshToken;

public sealed record RefreshTokenCommand(string RefreshToken) : ICommand<AuthTokens>;
