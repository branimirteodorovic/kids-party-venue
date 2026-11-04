namespace KidsPartyVenue.Domain.Providers;

/// <summary>Where a profile is in review. Suspension is separate (<see cref="Provider.SuspendedAtUtc"/>), so it keeps this status.</summary>
public enum ProviderStatus
{
    Draft,
    PendingReview,
    ChangesRequested,
    Published
}
