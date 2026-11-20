namespace KidsPartyVenue.Domain.Providers;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken);

    /// <summary>
    /// The slugs equal to <paramref name="baseSlug"/> or starting with "{baseSlug}-", which <see cref="ProviderSlug.MakeUnique"/> needs.
    /// </summary>
    Task<IReadOnlyList<string>> GetSlugsWithBaseAsync(string baseSlug, CancellationToken cancellationToken);

    /// <summary>
    /// The row version of a loaded provider, which the editor sends back so a change made in
    /// the meantime isn't overwritten. Saving a provider whose row changed since it was loaded fails as well.
    /// </summary>
    uint GetVersion(Provider provider);

    void Insert(Provider provider);
}
