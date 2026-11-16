using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.CreateProvider;

internal sealed class CreateProviderCommandHandler(
    ICityRepository cityRepository,
    IDistrictRepository districtRepository,
    IProviderRepository providerRepository,
    IProviderSlugGenerator slugGenerator,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateProviderCommand, CreateProviderResponse>
{
    public async Task<Result<CreateProviderResponse>> Handle(CreateProviderCommand command, CancellationToken cancellationToken)
    {
        if (!await cityRepository.ExistsAsync(command.CityId, cancellationToken))
        {
            return Result.Failure<CreateProviderResponse>(CityErrors.NotFound);
        }

        District? district = null;
        if (command.DistrictId is Guid districtId)
        {
            district = await districtRepository.GetByIdAsync(districtId, cancellationToken);
            if (district is null)
            {
                return Result.Failure<CreateProviderResponse>(DistrictErrors.NotFound);
            }
        }

        // A given district answers it already: either it is in the city, or Create rejects it as not in the city
        bool cityHasDistricts = district is not null
            || await districtRepository.AnyInCityAsync(command.CityId, cancellationToken);

        PhoneNumber? phone = null;
        if (!string.IsNullOrWhiteSpace(command.Phone))
        {
            Result<PhoneNumber> phoneResult = PhoneNumber.Create(command.Phone);
            if (phoneResult.IsFailure)
            {
                return Result.Failure<CreateProviderResponse>(phoneResult.Error);
            }

            phone = phoneResult.Value;
        }

        Result<GeoCoordinate> locationResult = GeoCoordinate.Create(command.Latitude, command.Longitude);
        if (locationResult.IsFailure)
        {
            return Result.Failure<CreateProviderResponse>(locationResult.Error);
        }

        string slug = await slugGenerator.GenerateAsync(command.Name, cancellationToken);

        Result<Provider> providerResult = Provider.Create(
            slug,
            command.Name,
            command.Category,
            command.Description,
            command.Address,
            command.CityId,
            district,
            cityHasDistricts,
            locationResult.Value,
            phone,
            isClaimed: false,
            dateTimeProvider.UtcNow);

        if (providerResult.IsFailure)
        {
            return Result.Failure<CreateProviderResponse>(providerResult.Error);
        }

        Provider provider = providerResult.Value;
        providerRepository.Insert(provider);

        await unitOfWork.SaveChangesAsync(cancellationToken);

        return new CreateProviderResponse(provider.Id, provider.Slug);
    }
}
