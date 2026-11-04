namespace KidsPartyVenue.Domain.Providers;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken);

    /// <summary>
    /// The slugs equal to <paramref name="baseSlug"/> or starting with "{baseSlug}-", which <see cref="ProviderSlug.MakeUnique"/> needs.
    /// </summary>
    Task<IReadOnlyList<string>> GetSlugsWithBaseAsync(string baseSlug, CancellationToken cancellationToken);

    void Insert(Provider provider);
}
