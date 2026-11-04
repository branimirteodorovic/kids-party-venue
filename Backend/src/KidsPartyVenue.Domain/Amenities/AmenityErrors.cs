using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Domain.Amenities;

public static class AmenityErrors
{
    public static readonly Error InvalidSlug = Error.Validation("Amenities.InvalidSlug", $"Slug must be lowercase letters and digits separated by hyphens, at most {UrlSlug.MaxLength} characters");
}
