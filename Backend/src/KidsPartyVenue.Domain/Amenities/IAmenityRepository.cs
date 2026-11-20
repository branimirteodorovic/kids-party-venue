namespace KidsPartyVenue.Domain.Amenities;

public interface IAmenityRepository
{
    /// <summary>The amenities with the given IDs; an unknown ID is left out, so the caller compares the counts.</summary>
    Task<IReadOnlyList<Amenity>> GetByIdsAsync(IReadOnlyCollection<Guid> amenityIds, CancellationToken cancellationToken);
}
