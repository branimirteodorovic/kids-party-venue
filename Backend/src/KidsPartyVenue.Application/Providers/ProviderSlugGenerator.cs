using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Application.Providers;

internal sealed class ProviderSlugGenerator(IProviderRepository providerRepository) : IProviderSlugGenerator
{
    public async Task<string> GenerateAsync(string name, CancellationToken cancellationToken)
    {
        string baseSlug = UrlSlug.FromName(name);
        IReadOnlyList<string> takenSlugs = await providerRepository.GetSlugsWithBaseAsync(baseSlug, cancellationToken);

        return ProviderSlug.MakeUnique(baseSlug, takenSlugs);
    }
}
