namespace KidsPartyVenue.Application.Caching;

public interface ICacheService
{
    ValueTask<T> GetOrCreateAsync<T>(ReadOnlySpan<char> key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);

    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, IEnumerable<string>? tags = null, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Invalidates every entry tagged with <paramref name="tag"/>. Call it after the write is saved.
    /// </summary>
    ValueTask RemoveByTagAsync(string tag, CancellationToken cancellationToken = default);
}
