namespace KidsPartyVenue.Domain.BookingPolicies;

/// <summary>
/// A provider's booking terms: the deposit, how many days after acceptance it is due, and until how many days
/// before the party a parent can cancel for free. 5.2 turns the numbers into dates for a booking.
/// </summary>
public sealed record BookingPolicy
{
    public const int MaxDepositPercentage = 100;
    public const decimal MaxDepositAmount = 100_000m;
    public const int MaxDepositDueDays = 30;
    public const int MaxFreeCancellationDays = 180;

    private BookingPolicy(DepositType depositType, decimal? depositValue, int? depositDueDays, int freeCancellationDays)
    {
        DepositType = depositType;
        DepositValue = depositValue;
        DepositDueDays = depositDueDays;
        FreeCancellationDays = freeCancellationDays;
    }

    public DepositType DepositType { get; }

    /// <summary>A whole percentage of the price, or a whole number of dinars; null without a deposit.</summary>
    public decimal? DepositValue { get; }

    /// <summary>Days after the provider accepts the inquiry; null without a deposit.</summary>
    public int? DepositDueDays { get; }

    /// <summary>Days before the party; 0 means until the day of the party.</summary>
    public int FreeCancellationDays { get; }

    public static Result<BookingPolicy> Create(
        DepositType depositType,
        decimal? depositValue,
        int? depositDueDays,
        int freeCancellationDays)
    {
        if (!Enum.IsDefined(depositType))
        {
            return Result.Failure<BookingPolicy>(BookingPolicyErrors.UnknownDepositType);
        }

        if (depositType == DepositType.None)
        {
            if (depositValue is not null || depositDueDays is not null)
            {
                return Result.Failure<BookingPolicy>(BookingPolicyErrors.DepositTermsWithoutDeposit);
            }
        }
        else
        {
            if (depositType == DepositType.Percentage && !IsWholeInRange(depositValue, MaxDepositPercentage))
            {
                return Result.Failure<BookingPolicy>(BookingPolicyErrors.InvalidDepositPercentage);
            }

            if (depositType == DepositType.FixedAmount && !IsWholeInRange(depositValue, MaxDepositAmount))
            {
                return Result.Failure<BookingPolicy>(BookingPolicyErrors.InvalidDepositAmount);
            }

            if (depositDueDays is not (>= 1 and <= MaxDepositDueDays))
            {
                return Result.Failure<BookingPolicy>(BookingPolicyErrors.InvalidDepositDueDays);
            }
        }

        if (freeCancellationDays is < 0 or > MaxFreeCancellationDays)
        {
            return Result.Failure<BookingPolicy>(BookingPolicyErrors.InvalidFreeCancellationDays);
        }

        return new BookingPolicy(depositType, depositValue, depositDueDays, freeCancellationDays);
    }

    private static bool IsWholeInRange(decimal? value, decimal max) =>
        value is { } v && v >= 1 && v <= max && decimal.Truncate(v) == v;
}
