namespace KidsPartyVenue.Domain.Providers;

public interface IProviderRepository
{
    Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken);

    void Insert(Provider provider);
}
