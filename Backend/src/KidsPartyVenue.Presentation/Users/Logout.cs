using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Users.Logout;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Users;

internal sealed class Logout : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/logout", async (
            HttpRequest request,
            HttpResponse httpResponse,
            ICommandHandler<LogoutCommand> handler,
            CancellationToken cancellationToken) =>
        {
            string? refreshToken = request.Cookies[RefreshTokenCookieHelper.Name];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await handler.Handle(new LogoutCommand(refreshToken), cancellationToken);
            }

            RefreshTokenCookieHelper.Clear(httpResponse);

            return Results.NoContent();
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }
}
