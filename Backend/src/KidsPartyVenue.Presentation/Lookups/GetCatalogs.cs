using KidsPartyVenue.Application.Lookups.GetCatalogs;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Lookups;

internal sealed class GetCatalogs : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("catalogs", async (
            IQueryHandler<GetCatalogsQuery, CatalogsResponse> handler,
            CancellationToken cancellationToken) =>
        {
            Result<CatalogsResponse> result = await handler.Handle(new GetCatalogsQuery(), cancellationToken);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
            .AllowAnonymous()
            .WithTags(Tags.Lookups);
    }
}
