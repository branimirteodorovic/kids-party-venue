using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Exceptions;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.UpdateProviderBasicInfo;

internal sealed class UpdateProviderBasicInfoCommandHandler(
    IProviderRepository providerRepository,
    IProviderPlaceLookup placeLookup,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider,
    ICacheService cacheService) : ICommandHandler<UpdateProviderBasicInfoCommand, ProviderVersionResponse>
{
    public async Task<Result<ProviderVersionResponse>> Handle(UpdateProviderBasicInfoCommand command, CancellationToken cancellationToken)
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

        Result<ProviderPlace> placeResult = await placeLookup.FindAsync(command.CityId, command.DistrictId, cancellationToken);
        if (placeResult.IsFailure)
        {
            return Result.Failure<ProviderVersionResponse>(placeResult.Error);
        }

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(command.Phone))
        {
            Result<PhoneNumber> phoneResult = PhoneNumber.Create(command.Phone);
            if (phoneResult.IsFailure)
            {
                return Result.Failure<ProviderVersionResponse>(phoneResult.Error);
            }

            phone = phoneResult.Value;
        }

        Result<GeoCoordinate> locationResult = GeoCoordinate.Create(command.Latitude, command.Longitude);
        if (locationResult.IsFailure)
        {
            return Result.Failure<ProviderVersionResponse>(locationResult.Error);
        }

        Result result = provider.ChangeBasicInfo(
            command.Name,
            command.Description,
            command.Address,
            command.CityId,
            placeResult.Value.District,
            placeResult.Value.CityHasDistricts,
            locationResult.Value,
            phone,
            dateTimeProvider.UtcNow);

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
