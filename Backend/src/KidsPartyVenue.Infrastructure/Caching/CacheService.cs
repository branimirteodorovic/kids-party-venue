using KidsPartyVenue.Application.Caching;
using Microsoft.Extensions.Caching.Hybrid;

namespace KidsPartyVenue.Infrastructure.Caching;

internal sealed class CacheService(HybridCache hybridCache) : ICacheService
{
    // HybridCache treats this tag as "every entry"
    private const string WildcardTag = "*";

    public ValueTask<T> GetOrCreateAsync<T>(ReadOnlySpan<char> key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        return hybridCache.GetOrCreateAsync(key, factory, options: new HybridCacheEntryOptions { Expiration = expiration }, tags: ValidateTags(tags), cancellationToken: cancellationToken);
    }

    public ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default)
    {
        return hybridCache.SetAsync(key, value, options: new HybridCacheEntryOptions { Expiration = expiration }, tags: ValidateTags(tags), cancellationToken: cancellationToken);
    }

    public ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        return hybridCache.RemoveAsync(key, cancellationToken);
    }

    public ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default)
    {
        ValidateTag(tag);
        return hybridCache.RemoveByTagAsync(tag, cancellationToken);
    }

    private static string[]? ValidateTags(IEnumerable<string>? tags)
    {
        if (tags is null)
        {
            return null;
        }

        string[] validated = [.. tags];
        foreach (string tag in validated)
        {
            ValidateTag(tag);
        }

        return validated;
    }

    private static void ValidateTag(string tag)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(tag);

        if (tag == WildcardTag)
        {
            throw new ArgumentException($"The tag '{WildcardTag}' is reserved by HybridCache.", nameof(tag));
        }
    }
}
