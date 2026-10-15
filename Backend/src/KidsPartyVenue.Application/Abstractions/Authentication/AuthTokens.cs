namespace KidsPartyVenue.Application.Abstractions.Authentication;

public sealed record AuthTokens(
    string AccessToken, DateTime AccessTokenExpiresAtUtc,
    string RefreshToken, DateTime RefreshTokenExpiresAtUtc);
