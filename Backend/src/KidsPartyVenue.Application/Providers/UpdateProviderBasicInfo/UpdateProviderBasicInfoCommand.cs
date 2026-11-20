using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.UpdateProviderBasicInfo;

/// <summary>Replaces the name, description, place and phone; the slug and category stay as they are.</summary>
public sealed record UpdateProviderBasicInfoCommand(
    Guid ProviderId,
    string Name,
    string? Description,
    string Address,
    Guid CityId,
    Guid? DistrictId,
    double Latitude,
    double Longitude,
    string? Phone,
    uint Version) : ICommand<ProviderVersionResponse>;
