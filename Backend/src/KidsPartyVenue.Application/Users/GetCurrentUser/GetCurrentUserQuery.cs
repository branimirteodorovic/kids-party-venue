using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Users.GetCurrentUser;

public sealed record GetCurrentUserQuery : IQuery<CurrentUserResponse>;
