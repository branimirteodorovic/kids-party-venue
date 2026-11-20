using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.RequestProviderChanges;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class RequestProviderChanges : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("admin/providers/{providerId:guid}/request-changes", async (
            Guid providerId,
            Request request,
            ICommandHandler<RequestProviderChangesCommand, ProviderVersionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<ProviderVersionResponse> result = await handler.Handle(
                new RequestProviderChangesCommand(providerId, request.Notes, request.Version),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ProvidersReview)
        .WithTags(Tags.Providers);
    }

    internal sealed class Request
    {
        /// <summary>What the owner should change; shown in the editor.</summary>
        public required string Notes { get; init; }

        /// <summary>The version from the last read or update.</summary>
        public required uint Version { get; init; }
    }
}
