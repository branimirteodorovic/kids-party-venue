using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.UpdateProviderAmenities;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class UpdateProviderAmenities : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("providers/{providerId:guid}/amenities", async (
            Guid providerId,
            Request request,
            ICommandHandler<UpdateProviderAmenitiesCommand, ProviderVersionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProviderAmenitiesCommand(providerId, request.AmenityIds, request.Version);

            Result<ProviderVersionResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(AuthorizationPolicies.ProviderMember)
        .WithTags(Tags.Providers);
    }

    internal sealed class Request
    {
        /// <summary>The whole set; an empty list clears it.</summary>
        public required List<Guid> AmenityIds { get; init; }

        /// <summary>The version from the last read or update.</summary>
        public required uint Version { get; init; }
    }
}
