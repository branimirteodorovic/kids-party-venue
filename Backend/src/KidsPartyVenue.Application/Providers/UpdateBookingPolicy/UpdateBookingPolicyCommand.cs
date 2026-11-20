using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain.BookingPolicies;

namespace KidsPartyVenue.Application.Providers.UpdateBookingPolicy;

/// <summary>Replaces the provider's booking terms as a whole.</summary>
public sealed record UpdateBookingPolicyCommand(
    Guid ProviderId,
    DepositType DepositType,
    decimal? DepositValue,
    int? DepositDueDays,
    int FreeCancellationDays,
    uint Version) : ICommand<ProviderVersionResponse>;
