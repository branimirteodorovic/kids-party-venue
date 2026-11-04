using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Providers;

internal sealed class ProviderRepository(KidsPartyVenueDbContext context) : IProviderRepository
{
    public Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken) =>
        context.Providers.SingleOrDefaultAsync(p => p.Id == providerId, cancellationToken);

    public async Task<IReadOnlyList<string>> GetSlugsWithBaseAsync(string baseSlug, CancellationToken cancellationToken) =>
        await context.Providers
            .Where(p => p.Slug == baseSlug || p.Slug.StartsWith(baseSlug + "-"))
            .Select(p => p.Slug)
            .ToListAsync(cancellationToken);

    public void Insert(Provider provider) => context.Providers.Add(provider);
}
