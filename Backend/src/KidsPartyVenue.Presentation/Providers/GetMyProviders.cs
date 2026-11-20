using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers.GetMyProviders;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class GetMyProviders : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/me/providers", async (
            IQueryHandler<GetMyProvidersQuery, List<MyProviderResponse>> handler,
            CancellationToken cancellationToken) =>
        {
            Result<List<MyProviderResponse>> result = await handler.Handle(new GetMyProvidersQuery(), cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.UsersReadSelf)
        .WithTags(Tags.Providers);
    }
}
