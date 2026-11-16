using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Cities;

internal sealed class DistrictRepository(KidsPartyVenueDbContext context) : IDistrictRepository
{
    public Task<District?> GetByIdAsync(Guid districtId, CancellationToken cancellationToken) =>
        context.Districts.SingleOrDefaultAsync(d => d.Id == districtId, cancellationToken);

    public Task<bool> AnyInCityAsync(Guid cityId, CancellationToken cancellationToken) =>
        context.Districts.AnyAsync(d => d.CityId == cityId, cancellationToken);
}
