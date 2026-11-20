using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Exceptions;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.Application.Providers.UpdateVenueDetails;

internal sealed class UpdateVenueDetailsCommandHandler(
    IProviderRepository providerRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICacheService cacheService) : ICommandHandler<UpdateVenueDetailsCommand, ProviderVersionResponse>
{
    public async Task<Result<ProviderVersionResponse>> Handle(UpdateVenueDetailsCommand command, CancellationToken cancellationToken)
    {
        Provider? provider = await providerRepository.GetByIdAsync(command.ProviderId, cancellationToken);
        if (provider is null)
        {
            return Result.Failure<ProviderVersionResponse>(ProviderErrors.NotFound);
        }

        if (providerRepository.GetVersion(provider) != command.Version)
        {
            return Result.Failure<ProviderVersionResponse>(ProviderErrors.ConcurrencyConflict);
        }

        Result<VenueDetails> detailsResult = VenueDetails.Create(
            command.MaxChildren,
            command.MaxAdults,
            command.MinAge,
            command.MaxAge,
            command.OutsideCatering,
            command.OutsideCake,
            command.FoodService,
            command.HasOwnAnimator,
            command.HasOwnPhotographer,
            command.HasSmokingArea);

        if (detailsResult.IsFailure)
        {
            return Result.Failure<ProviderVersionResponse>(detailsResult.Error);
        }

        Result result = provider.ChangeVenueDetails(detailsResult.Value, dateTimeProvider.UtcNow);
        if (result.IsFailure)
        {
            return Result.Failure<ProviderVersionResponse>(result.Error);
        }

        try
        {
            await unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            return Result.Failure<ProviderVersionResponse>(ProviderErrors.ConcurrencyConflict);
        }

        await cacheService.RemoveByTagAsync(ProviderCache.Tag(provider.Id), cancellationToken);

        return new ProviderVersionResponse(providerRepository.GetVersion(provider));
    }
}
