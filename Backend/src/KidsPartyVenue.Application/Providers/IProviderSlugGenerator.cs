namespace KidsPartyVenue.Application.Providers;

public interface IProviderSlugGenerator
{
    /// <summary>
    /// A slug for a new provider with this name that no saved provider has and that isn't reserved.
    /// </summary>
    Task<string> GenerateAsync(string name, CancellationToken cancellationToken);
}
