using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers.RegisterProvider;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class RegisterProvider : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPost("providers", async (
            Request request,
            ICommandHandler<RegisterProviderCommand, RegisterProviderResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new RegisterProviderCommand(
                request.Name,
                request.Category,
                request.Description,
                request.Address,
                request.CityId,
                request.DistrictId,
                request.Latitude,
                request.Longitude,
                request.Phone);

            Result<RegisterProviderResponse> result = await handler.Handle(command, cancellationToken);

            // The provider editor read (1.9), which the new owner can open right away
            return result.Match(response => Results.Created($"/providers/{response.Id}", response), ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ProvidersRegister)
        .WithTags(Tags.Providers);
    }

    internal sealed class Request
    {
        public required string Name { get; init; }

        public required ProviderCategory Category { get; init; }

        public string? Description { get; init; }

        public required string Address { get; init; }

        public required Guid CityId { get; init; }

        /// <summary>Required in a city that has districts; omitted in one without any.</summary>
        public Guid? DistrictId { get; init; }

        public required double Latitude { get; init; }

        public required double Longitude { get; init; }

        public string? Phone { get; init; }
    }
}
