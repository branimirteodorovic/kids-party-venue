using KidsPartyVenue.Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace KidsPartyVenue.Infrastructure.Caching;

internal sealed class CacheService(HybridCache hybridCache) : ICacheService
{
    public ValueTask<T> GetOrCreateAsync<T>(ReadOnlySpan<char> key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        return hybridCache.GetOrCreateAsync(key, factory, options: new HybridCacheEntryOptions { Expiration = expiration }, cancellationToken: cancellationToken);
    }

    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default)
    {
        return hybridCache.SetAsync(key, value, options: new HybridCacheEntryOptions { Expiration = expiration }, cancellationToken: cancellationToken);
    }

    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return hybridCache.RemoveAsync(key, cancellationToken);
    }
}