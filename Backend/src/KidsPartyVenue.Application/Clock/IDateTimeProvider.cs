namespace KidsPartyVenue.Application.Clock;

public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
