using KidsPartyVenue.Domain.Phones;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace KidsPartyVenue.Infrastructure.Phones;

/// <summary>Stores a <see cref="PhoneNumber"/> as its E.164 text.</summary>
internal sealed class PhoneNumberConverter()
    : ValueConverter<PhoneNumber, string>(p => p.Value, s => FromText(s))
{
    // Only valid numbers are stored, so a failure here is a data fault and may throw
    private static PhoneNumber FromText(string text) => PhoneNumber.Create(text).Value;
}
