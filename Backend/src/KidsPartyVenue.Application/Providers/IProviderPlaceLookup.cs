using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Providers;

public interface IProviderPlaceLookup
{
    /// <summary>
    /// Finds the city and district a provider is placed in, failing with <c>Cities.NotFound</c> or
    /// <c>Districts.NotFound</c>. Whether the district belongs to the city is checked by the provider itself.
    /// </summary>
    Task<Result<ProviderPlace>> FindAsync(Guid cityId, Guid? districtId, CancellationToken cancellationToken);
}
