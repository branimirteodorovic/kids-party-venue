using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.SubmitProviderForReview;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class SubmitProviderForReview : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("providers/{providerId:guid}/submit", async (
            Guid providerId,
            Request request,
            ICommandHandler<SubmitProviderForReviewCommand, ProviderVersionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<ProviderVersionResponse> result = await handler.Handle(
                new SubmitProviderForReviewCommand(providerId, request.Version),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(AuthorizationPolicies.ProviderMember)
        .WithTags(Tags.Providers);
    }

    internal sealed class Request
    {
        /// <summary>The version from the last read or update.</summary>
        public required uint Version { get; init; }
    }
}
