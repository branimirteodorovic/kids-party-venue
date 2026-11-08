using System.Globalization;

namespace KidsPartyVenue.Domain.BookingPolicies;

public static class BookingPolicyErrors
{
    public static readonly Error UnknownDepositType = Error.Validation("BookingPolicies.UnknownDepositType", "Deposit type is not supported");
    public static readonly Error DepositTermsWithoutDeposit = Error.Validation("BookingPolicies.DepositTermsWithoutDeposit", "A deposit value and due days can only be given with a deposit");
    public static readonly Error InvalidDepositPercentage = Error.Validation("BookingPolicies.InvalidDepositPercentage", $"Deposit percentage must be a whole number from 1 to {BookingPolicy.MaxDepositPercentage}");
    public static readonly Error InvalidDepositAmount = Error.Validation("BookingPolicies.InvalidDepositAmount", string.Create(CultureInfo.InvariantCulture, $"Deposit amount must be a whole number of dinars from 1 to {BookingPolicy.MaxDepositAmount:N0}"));
    public static readonly Error InvalidDepositDueDays = Error.Validation("BookingPolicies.InvalidDepositDueDays", $"Deposit due days must be from 1 to {BookingPolicy.MaxDepositDueDays}");
    public static readonly Error InvalidFreeCancellationDays = Error.Validation("BookingPolicies.InvalidFreeCancellationDays", $"Free cancellation days must be from 0 to {BookingPolicy.MaxFreeCancellationDays}");
}
