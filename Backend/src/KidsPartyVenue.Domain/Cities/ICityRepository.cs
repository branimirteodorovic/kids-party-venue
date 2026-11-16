namespace KidsPartyVenue.Domain.Cities;

public interface ICityRepository
{
    Task<bool> ExistsAsync(Guid cityId, CancellationToken cancellationToken);
}
