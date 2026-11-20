using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers.GetProviderForEditing;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class GetProviderForEditing : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // The guid constraint keeps this apart from the public GET providers/{slug} (1.11)
        app.MapGet("providers/{providerId:guid}", async (
            Guid providerId,
            IQueryHandler<GetProviderForEditingQuery, ProviderForEditingResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<ProviderForEditingResponse> result = await handler.Handle(
                new GetProviderForEditingQuery(providerId),
                cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(AuthorizationPolicies.ProviderMember)
        .WithTags(Tags.Providers);
    }
}
