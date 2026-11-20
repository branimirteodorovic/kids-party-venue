using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.UpdateVenueDetails;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Venues;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class UpdateVenueDetails : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("providers/{providerId:guid}/venue-details", async (
            Guid providerId,
            Request request,
            ICommandHandler<UpdateVenueDetailsCommand, ProviderVersionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateVenueDetailsCommand(
                providerId,
                request.MaxChildren,
                request.MaxAdults,
                request.MinAge,
                request.MaxAge,
                request.OutsideCatering,
                request.OutsideCake,
                request.FoodService,
                request.HasOwnAnimator,
                request.HasOwnPhotographer,
                request.HasSmokingArea,
                request.Version);

            Result<ProviderVersionResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(AuthorizationPolicies.ProviderMember)
        .WithTags(Tags.Providers);
    }

    internal sealed class Request
    {
        public required int MaxChildren { get; init; }

        /// <summary>Omitted when the venue sets no separate limit for adults.</summary>
        public int? MaxAdults { get; init; }

        public required int MinAge { get; init; }

        public required int MaxAge { get; init; }

        public required OutsideFoodRule OutsideCatering { get; init; }

        public required OutsideFoodRule OutsideCake { get; init; }

        public required FoodService FoodService { get; init; }

        public required bool HasOwnAnimator { get; init; }

        public required bool HasOwnPhotographer { get; init; }

        public required bool HasSmokingArea { get; init; }

        /// <summary>The version from the last read or update.</summary>
        public required uint Version { get; init; }
    }
}
