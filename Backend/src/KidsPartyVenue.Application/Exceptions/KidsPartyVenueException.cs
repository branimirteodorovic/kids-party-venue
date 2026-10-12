using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Exceptions;

public sealed class KidsPartyVenueException : Exception
{
    public KidsPartyVenueException(string requestName, Error? error = default, Exception? innerException = default)
        : base("Application exception", innerException)
    {
        RequestName = requestName;
        Error = error;
    }

    public string RequestName { get; }

    public Error? Error { get; }
}
