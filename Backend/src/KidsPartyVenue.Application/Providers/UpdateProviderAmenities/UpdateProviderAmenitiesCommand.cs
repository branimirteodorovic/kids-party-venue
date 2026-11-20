using KidsPartyVenue.Application.Messaging;

namespace KidsPartyVenue.Application.Providers.UpdateProviderAmenities;

/// <summary>Replaces the provider's amenities with the given set; an empty list clears it.</summary>
public sealed record UpdateProviderAmenitiesCommand(
    Guid ProviderId,
    IReadOnlyList<Guid> AmenityIds,
    uint Version) : ICommand<ProviderVersionResponse>;
