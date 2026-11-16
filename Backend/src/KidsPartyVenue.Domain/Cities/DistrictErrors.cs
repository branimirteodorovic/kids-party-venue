using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Domain.Cities;

public static class DistrictErrors
{
    public static readonly Error NotFound = Error.NotFound("Districts.NotFound", "District is not found");
    public static readonly Error InvalidSlug = Error.Validation("Districts.InvalidSlug", $"Slug must be lowercase letters and digits separated by hyphens, at most {UrlSlug.MaxLength} characters");
}
