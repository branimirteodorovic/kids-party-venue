using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.RegisterProvider;

/// <summary>A logged-in user creates a claimed Draft profile and becomes its Owner; the slug is generated from the name.</summary>
public sealed record RegisterProviderCommand(
    string Name,
    ProviderCategory Category,
    string? Description,
    string Address,
    Guid CityId,
    Guid? DistrictId,
    double Latitude,
    double Longitude,
    string? Phone) : ICommand<RegisterProviderResponse>;
