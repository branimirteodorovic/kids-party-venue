using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.SuspendProvider;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class SuspendProvider : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/providers/{providerId:guid}/suspend", async (
            Guid providerId,
            Request request,
            ICommandHandler<SuspendProviderCommand, ProviderVersionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<ProviderVersionResponse> result = await handler.Handle(
                new SuspendProviderCommand(providerId, request.Reason, request.Version),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ProvidersReview)
        .WithTags(Tags.Providers);
    }

    internal sealed class Request
    {
        /// <summary>Why the profile is hidden; shown to its owner.</summary>
        public required string Reason { get; init; }

        /// <summary>The version from the last read or update.</summary>
        public required uint Version { get; init; }
    }
}
