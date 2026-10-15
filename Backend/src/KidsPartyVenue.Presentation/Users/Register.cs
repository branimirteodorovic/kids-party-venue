using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Users.Register;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Users;

internal sealed class Register : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("auth/register", async (
            Request request,
            ICommandHandler<RegisterUserCommand, Guid> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterUserCommand(request.Email, request.Password, request.FullName);

            Result<Guid> result = await handler.Handle(command, cancellationToken);

            return result.Match(userId => Results.Ok(new Response { UserId = userId }), ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        public required string Email { get; init; }

        public required string Password { get; init; }

        public required string FullName { get; init; }
    }

    internal sealed class Response
    {
        public required Guid UserId { get; init; }
    }
}
