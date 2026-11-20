using KidsPartyVenue.Domain.BookingPolicies;

namespace KidsPartyVenue.Application.Providers.GetProviderForEditing;

public sealed record BookingPolicyResponse(
    DepositType DepositType,
    decimal? DepositValue,
    int? DepositDueDays,
    int FreeCancellationDays);
