using System.Text.RegularExpressions;

namespace KidsPartyVenue.Domain.Phones;

/// <summary>
/// A phone number in E.164 form ("+381641234567"). Serbian local numbers ("064/123-4567") get the +381 prefix.
/// </summary>
public sealed partial record PhoneNumber
{
    public const int MaxLength = 16;

    private const string SerbiaPrefix = "+381";

    private PhoneNumber(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static Result<PhoneNumber> Create(string? number)
    {
        string digits = SeparatorRegex().Replace(number ?? string.Empty, string.Empty);

        if (digits.StartsWith("00", StringComparison.Ordinal))
        {
            digits = "+" + digits[2..];
        }
        else if (digits.StartsWith('0'))
        {
            digits = SerbiaPrefix + digits[1..];
        }

        if (!E164Regex().IsMatch(digits))
        {
            return Result.Failure<PhoneNumber>(PhoneErrors.InvalidNumber);
        }

        return new PhoneNumber(digits);
    }

    [GeneratedRegex(@"[\s\-/.()]")]
    private static partial Regex SeparatorRegex();

    [GeneratedRegex(@"^\+[1-9][0-9]{7,14}$")]
    private static partial Regex E164Regex();
}
