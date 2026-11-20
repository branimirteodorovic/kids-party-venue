using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.Application.Providers;

public sealed record VenueDetailsResponse(
    int MaxChildren,
    int? MaxAdults,
    int MinAge,
    int MaxAge,
    OutsideFoodRule OutsideCatering,
    OutsideFoodRule OutsideCake,
    FoodService FoodService,
    bool HasOwnAnimator,
    bool HasOwnPhotographer,
    bool HasSmokingArea);
