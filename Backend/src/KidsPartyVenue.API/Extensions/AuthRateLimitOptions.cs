using System.ComponentModel.DataAnnotations;

namespace KidsPartyVenue.API.Extensions;

internal sealed class AuthRateLimitOptions
{
    public const string SectionName = "RateLimiting:Auth";

    [Range(1, int.MaxValue)]
    public int PermitLimit { get; set; } = 10;

    [Range(1, int.MaxValue)]
    public int WindowSeconds { get; set; } = 60;
}
