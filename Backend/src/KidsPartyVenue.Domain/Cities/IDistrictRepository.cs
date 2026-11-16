namespace KidsPartyVenue.Domain.Cities;

public interface IDistrictRepository
{
    Task<District?> GetByIdAsync(Guid districtId, CancellationToken cancellationToken);

    /// <summary>Whether the city has any districts, which decides if a provider there needs one.</summary>
    Task<bool> AnyInCityAsync(Guid cityId, CancellationToken cancellationToken);
}
