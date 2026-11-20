using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.GetMyProviders;

public sealed record MyProviderResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ProviderStatus Status { get; init; }
    public bool IsSuspended { get; init; }
    public ProviderMemberRole Role { get; init; }
}
