using KidsPartyVenue.Domain.Cities;

namespace KidsPartyVenue.Application.Providers;

/// <summary>The district a provider is placed in, and whether its city has districts, which decides if one is required.</summary>
public sealed record ProviderPlace(District? District, bool CityHasDistricts);
