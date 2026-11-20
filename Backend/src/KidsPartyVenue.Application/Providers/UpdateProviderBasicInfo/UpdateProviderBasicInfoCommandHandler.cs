using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Exceptions;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.UpdateProviderBasicInfo;

internal sealed class UpdateProviderBasicInfoCommandHandler(
    IProviderRepository providerRepository,
    ICityRepository cityRepository,
    IDistrictRepository districtRepository,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<UpdateProviderBasicInfoCommand, ProviderVersionResponse>
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

        if (!await cityRepository.ExistsAsync(command.CityId, cancellationToken))
        {
            return Result.Failure<ProviderVersionResponse>(CityErrors.NotFound);
        }

        District? district = null;
        if (command.DistrictId is Guid districtId)
        {
            district = await districtRepository.GetByIdAsync(districtId, cancellationToken);
            if (district is null)
            {
                return Result.Failure<ProviderVersionResponse>(DistrictErrors.NotFound);
            }
        }

        // A given district answers it already: either it is in the city, or ChangeBasicInfo rejects it as not in the city
        bool cityHasDistricts = district is not null
            || await districtRepository.AnyInCityAsync(command.CityId, cancellationToken);

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
            district,
            cityHasDistricts,
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

        return new ProviderVersionResponse(providerRepository.GetVersion(provider));
    }
}
