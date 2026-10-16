using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Users.GetCurrentUser;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Users;

internal sealed class GetCurrentUser : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me", async (
            IQueryHandler<GetCurrentUserQuery, CurrentUserResponse> handler,
            CancellationToken cancellationToken) => 
        {
            var result = await handler.Handle(new GetCurrentUserQuery(), cancellationToken);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
            .RequireAuthorization(Permissions.UsersReadSelf)
            .WithTags(Tags.Users);
    }
}
