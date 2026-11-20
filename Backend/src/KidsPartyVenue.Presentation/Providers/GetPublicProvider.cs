using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers.GetPublicProvider;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class GetPublicProvider : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        // The editor's providers/{providerId:guid} is more specific, so a GUID never reaches this route
        app.MapGet("providers/{slug}", async (
            string slug,
            IQueryHandler<GetPublicProviderQuery, PublicProviderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<PublicProviderResponse> result = await handler.Handle(new GetPublicProviderQuery(slug), cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .AllowAnonymous()
        .WithTags(Tags.Providers);
    }
}
