using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Amenities;

internal sealed class AmenityRepository(KidsPartyVenueDbContext context) : IAmenityRepository
{
    public async Task<IReadOnlyList<Amenity>> GetByIdsAsync(IReadOnlyCollection<Guid> amenityIds, CancellationToken cancellationToken) =>
        await context.Amenities
            .Where(a => amenityIds.Contains(a.Id))
            .ToListAsync(cancellationToken);
}
