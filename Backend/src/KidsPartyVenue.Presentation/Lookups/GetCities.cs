using KidsPartyVenue.Application.Lookups.GetCities;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Lookups;

internal sealed class GetCities : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("cities", async (
            IQueryHandler<GetCitiesQuery, List<CityResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<CityResponse>> result = await handler.Handle(new GetCitiesQuery(), cancellationToken);
            return result.Match(Results.Ok, ApiResults.Problem);
        })
            .AllowAnonymous()
            .WithTags(Tags.Lookups);
    }
}
