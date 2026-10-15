using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Users.Login;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Users;

internal sealed class Login : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/login", async (
            Request request,
            HttpResponse httpResponse,
            ICommandHandler<LoginUserCommand, AuthTokens> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new LoginUserCommand(request.Email, request.Password);

            Result<AuthTokens> result = await handler.Handle(command, cancellationToken);

            IResult response = result.Match(
                tokens => Results.Ok(new Response
                {
                    AccessToken = tokens.AccessToken,
                    AccessTokenExpiresAtUtc = tokens.AccessTokenExpiresAtUtc
                }),
                ApiResults.Problem);

            if (result.IsSuccess)
            {
                RefreshTokenCookieHelper.Set(httpResponse, result.Value.RefreshToken, result.Value.RefreshTokenExpiresAtUtc);
            }

            return response;
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        public required string Email { get; init; }

        public required string Password { get; init; }
    }

    internal sealed class Response
    {
        public required string AccessToken { get; init; }

        public required DateTime AccessTokenExpiresAtUtc { get; init; }
    }
}
