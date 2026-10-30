using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Users.ChangePreferredLanguage;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Users;

internal sealed class ChangePreferredLanguage : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("users/me/language", async (
            Request request,
            ICommandHandler<ChangePreferredLanguageCommand> handler,
            CancellationToken cancellationToken) =>
        {
            Result result = await handler.Handle(new ChangePreferredLanguageCommand(request.Language), cancellationToken);

            return result.Match(Results.NoContent, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.UsersUpdateSelf)
        .WithTags(Tags.Users);
    }

    internal sealed class Request
    {
        public required Language Language { get; init; }
    }
}
