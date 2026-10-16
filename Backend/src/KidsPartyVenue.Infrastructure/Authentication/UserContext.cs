using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Exceptions;
using Microsoft.AspNetCore.Http;

namespace KidsPartyVenue.Infrastructure.Authentication;

internal sealed class UserContext(IHttpContextAccessor httpContextAccessor) : IUserContext
{
    public Guid UserId => httpContextAccessor.HttpContext?.User.GetUserId()
        ?? throw new KidsPartyVenueException("User context is unavailable");
}
