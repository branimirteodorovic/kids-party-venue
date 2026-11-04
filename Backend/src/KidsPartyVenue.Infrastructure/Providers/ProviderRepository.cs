using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Providers;

internal sealed class ProviderRepository(KidsPartyVenueDbContext context) : IProviderRepository
{
    public Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken) =>
        context.Providers.SingleOrDefaultAsync(p => p.Id == providerId, cancellationToken);

    public void Insert(Provider provider) => context.Providers.Add(provider);
}
