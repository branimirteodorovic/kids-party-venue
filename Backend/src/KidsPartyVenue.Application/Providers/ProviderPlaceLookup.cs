using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Cities;

namespace KidsPartyVenue.Application.Providers;

internal sealed class ProviderPlaceLookup(
    ICityRepository cityRepository,
    IDistrictRepository districtRepository) : IProviderPlaceLookup
{
    public async Task<Result<ProviderPlace>> FindAsync(Guid cityId, Guid? districtId, CancellationToken cancellationToken)
    {
        if (!await cityRepository.ExistsAsync(cityId, cancellationToken))
        {
            return Result.Failure<ProviderPlace>(CityErrors.NotFound);
        }

        District? district = null;
        if (districtId is Guid id)
        {
            district = await districtRepository.GetByIdAsync(id, cancellationToken);
            if (district is null)
            {
                return Result.Failure<ProviderPlace>(DistrictErrors.NotFound);
            }
        }

        // A given district answers it already: either it is in the city, or the provider rejects it as not in the city
        bool cityHasDistricts = district is not null
            || await districtRepository.AnyInCityAsync(cityId, cancellationToken);

        return new ProviderPlace(district, cityHasDistricts);
    }
}
