using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Exceptions;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.UpdateProviderAmenities;

internal sealed class UpdateProviderAmenitiesCommandHandler(
    IProviderRepository providerRepository,
    IAmenityRepository amenityRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICacheService cacheService) : ICommandHandler<UpdateProviderAmenitiesCommand, ProviderVersionResponse>
{
    public async Task<Result<ProviderVersionResponse>> Handle(UpdateProviderAmenitiesCommand command, CancellationToken cancellationToken)
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

        Guid[] amenityIds = [.. command.AmenityIds.Distinct()];
        IReadOnlyList<Amenity> amenities = await amenityRepository.GetByIdsAsync(amenityIds, cancellationToken);
        if (amenities.Count != amenityIds.Length)
        {
            return Result.Failure<ProviderVersionResponse>(AmenityErrors.NotFound);
        }

        Result result = provider.ChangeAmenities(amenities, dateTimeProvider.UtcNow);
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
