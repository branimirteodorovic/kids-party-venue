namespace KidsPartyVenue.Application.Caching;

public interface ICacheService
{
    ValueTask<T> GetOrCreateAsync<T>(ReadOnlySpan<char> key, Func<CancellationToken, ValueTask<T>> factory, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    ValueTask SetAsync<T>(string key, T value, TimeSpan? expiration = null, CancellationToken cancellationToken = default);

    ValueTask RemoveAsync(string key, CancellationToken cancellationToken = default);
}
