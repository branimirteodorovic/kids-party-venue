using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.UpdateBookingPolicy;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Presentation.Providers;

internal sealed class UpdateBookingPolicy : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapPut("providers/{providerId:guid}/booking-policy", async (
            Guid providerId,
            Request request,
            ICommandHandler<UpdateBookingPolicyCommand, ProviderVersionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateBookingPolicyCommand(
                providerId,
                request.DepositType,
                request.DepositValue,
                request.DepositDueDays,
                request.FreeCancellationDays,
                request.Version);

            Result<ProviderVersionResponse> result = await handler.Handle(command, cancellationToken);

            return result.Match(Results.Ok, ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.ProvidersManageAny)
        .WithTags(Tags.Providers);
    }

    internal sealed class Request
    {
        public required DepositType DepositType { get; init; }

        /// <summary>A whole percentage or a whole number of dinars; omitted without a deposit.</summary>
        public decimal? DepositValue { get; init; }

        /// <summary>Days after the provider accepts; omitted without a deposit.</summary>
        public int? DepositDueDays { get; init; }

        public required int FreeCancellationDays { get; init; }

        /// <summary>The version from the last read or update.</summary>
        public required uint Version { get; init; }
    }
}
