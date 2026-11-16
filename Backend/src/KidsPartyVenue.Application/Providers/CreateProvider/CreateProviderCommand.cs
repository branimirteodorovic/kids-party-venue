using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.CreateProvider;

/// <summary>An admin creates an unclaimed Draft profile; the slug is generated from the name.</summary>
public sealed record CreateProviderCommand(
    string Name,
    ProviderCategory Category,
    string? Description,
    string Address,
    Guid CityId,
    Guid? DistrictId,
    double Latitude,
    double Longitude,
    string? Phone) : ICommand<CreateProviderResponse>;
