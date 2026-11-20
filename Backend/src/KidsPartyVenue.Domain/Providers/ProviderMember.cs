namespace KidsPartyVenue.Domain.Providers;

/// <summary>A user who runs a provider, referred to by ID so the user stays outside the aggregate.</summary>
public sealed class ProviderMember : Entity
{
    private ProviderMember()
    {
    }

    public Guid ProviderId { get; private set; }

    public Guid UserId { get; private set; }

    public ProviderMemberRole Role { get; private set; }

    public DateTime JoinedAtUtc { get; private set; }

    internal static ProviderMember Create(Guid providerId, Guid userId, ProviderMemberRole role, DateTime joinedAtUtc) =>
        new()
        {
            ProviderId = providerId,
            UserId = userId,
            Role = role,
            JoinedAtUtc = joinedAtUtc
        };
}
