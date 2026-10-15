using System.ComponentModel.DataAnnotations;

namespace KidsPartyVenue.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public const string SectionName = "Authentication";
    [Required] public string Issuer { get; init; } = string.Empty;
    [Required] public string Audience { get; init; } = string.Empty;
    [Required, MinLength(32)] public string SigningKey { get; init; } = string.Empty;
    public int AccessTokenLifetimeMinutes { get; init; } = 15;
    public int RefreshTokenLifetimeDays { get; init; } = 30;
}