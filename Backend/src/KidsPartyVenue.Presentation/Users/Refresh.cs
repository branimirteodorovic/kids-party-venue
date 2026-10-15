using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Users.RefreshToken;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Users;

internal sealed class Refresh : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/refresh", async (
            HttpRequest request,
            HttpResponse httpResponse,
            ICommandHandler<RefreshTokenCommand, AuthTokens> handler,
            CancellationToken cancellationToken
            ) =>
            {
                string refreshToken = request.Cookies[RefreshTokenCookieHelper.Name] ?? string.Empty;

                Result<AuthTokens> result = await handler.Handle(new RefreshTokenCommand(refreshToken), cancellationToken);

                IResult response = result.Match(tokens => Results.Ok(new Response
                {
                    AccessToken = tokens.AccessToken,
                    AccessTokenExpiresAtUtc = tokens.AccessTokenExpiresAtUtc
                }),

                ApiResults.Problem);

                if (result.IsSuccess)
                {
                    RefreshTokenCookieHelper.Set(httpResponse, result.Value.RefreshToken, result.Value.RefreshTokenExpiresAtUtc);
                }
                else
                {
                    RefreshTokenCookieHelper.Clear(httpResponse);
                }

                return response;
            })
            .AllowAnonymous()
            .WithTags(Tags.Users);
    }

    internal sealed class Response
    {
        public required string AccessToken { get; init; }

        public required DateTime AccessTokenExpiresAtUtc { get; init; }
    }
}
