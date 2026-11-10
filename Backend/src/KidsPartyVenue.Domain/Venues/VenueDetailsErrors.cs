namespace KidsPartyVenue.Domain.Venues;

public static class VenueDetailsErrors
{
    public static readonly Error InvalidMaxChildren = Error.Validation("VenueDetails.InvalidMaxChildren", $"Maximum number of children must be from 1 to {VenueDetails.GuestLimit}");
    public static readonly Error InvalidMaxAdults = Error.Validation("VenueDetails.InvalidMaxAdults", $"Maximum number of adults must be from 0 to {VenueDetails.GuestLimit}");
    public static readonly Error InvalidAge = Error.Validation("VenueDetails.InvalidAge", $"Ages must be from 0 to {VenueDetails.AgeLimit} years");
    public static readonly Error InvalidAgeRange = Error.Validation("VenueDetails.InvalidAgeRange", "Minimum age can't be greater than maximum age");
    public static readonly Error UnknownOutsideFoodRule = Error.Validation("VenueDetails.UnknownOutsideFoodRule", "Outside food rule is not supported");
    public static readonly Error UnknownFoodService = Error.Validation("VenueDetails.UnknownFoodService", "Food service is not supported");
}
