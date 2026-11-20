using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.UpdateProviderBasicInfo;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class UpdateProviderBasicInfo : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("providers/{providerId:guid}/basic-info", async (
            Guid providerId,
            Request request,
            ICommandHandler<UpdateProviderBasicInfoCommand, ProviderVersionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateProviderBasicInfoCommand(
                providerId,
                request.Name,
                request.Description,
                request.Address,
                request.CityId,
                request.DistrictId,
                request.Latitude,
                request.Longitude,
                request.Phone,
                request.Version);

            Result<ProviderVersionResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ProvidersManageAny)
        .WithTags(Tags.Providers);
    }

    internal sealed class Request
    {
        public required string Name { get; init; }

        public string? Description { get; init; }

        public required string Address { get; init; }

        public required Guid CityId { get; init; }

        /// <summary>Required in a city that has districts; omitted in one without any.</summary>
        public Guid? DistrictId { get; init; }

        public required double Latitude { get; init; }

        public required double Longitude { get; init; }

        /// <summary>Omitted or blank to remove the phone.</summary>
        public string? Phone { get; init; }

        /// <summary>The version from the last read or update.</summary>
        public required uint Version { get; init; }
    }
}
