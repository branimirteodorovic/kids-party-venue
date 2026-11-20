using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.Application.Providers.UpdateVenueDetails;

/// <summary>Replaces a venue's details as a whole.</summary>
public sealed record UpdateVenueDetailsCommand(
    Guid ProviderId,
    int MaxChildren,
    int? MaxAdults,
    int MinAge,
    int MaxAge,
    OutsideFoodRule OutsideCatering,
    OutsideFoodRule OutsideCake,
    FoodService FoodService,
    bool HasOwnAnimator,
    bool HasOwnPhotographer,
    bool HasSmokingArea,
    uint Version) : ICommand<ProviderVersionResponse>;
