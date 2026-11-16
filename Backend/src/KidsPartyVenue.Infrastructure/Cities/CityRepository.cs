using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Cities;

internal sealed class CityRepository(KidsPartyVenueDbContext context) : ICityRepository
{
    public Task<bool> ExistsAsync(Guid cityId, CancellationToken cancellationToken) =>
        context.Cities.AnyAsync(c => c.Id == cityId, cancellationToken);
}
