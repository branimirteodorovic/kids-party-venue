using KidsPartyVenue.Application.Clock;

namespace KidsPartyVenue.Infrastructure.Clock;

internal sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
