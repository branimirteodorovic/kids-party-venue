using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.CreateProvider;

internal sealed class CreateProviderCommandHandler(
    IProviderRepository providerRepository,
    IProviderPlaceLookup placeLookup,
    IProviderSlugGenerator slugGenerator,
    IUnitOfWork unitOfWork,
    IDateTimeProvider dateTimeProvider) : ICommandHandler<CreateProviderCommand, CreateProviderResponse>
{
    public async Task<Result<CreateProviderResponse>> Handle(CreateProviderCommand command, CancellationToken cancellationToken)
    {
        Result<ProviderPlace> placeResult = await placeLookup.FindAsync(command.CityId, command.DistrictId, cancellationToken);
        if (placeResult.IsFailure)
        {
            return Result.Failure<CreateProviderResponse>(placeResult.Error);
        }

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
            placeResult.Value.District,
            placeResult.Value.CityHasDistricts,
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
