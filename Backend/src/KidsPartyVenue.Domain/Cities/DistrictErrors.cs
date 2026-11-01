using KidsPartyVenue.Domain.Lookups;

namespace KidsPartyVenue.Domain.Cities;

public static class DistrictErrors
{
    public static readonly Error InvalidSlug = Error.Validation("Districts.InvalidSlug", $"Slug must be lowercase letters and digits separated by hyphens, at most {LookupSlug.MaxLength} characters");
}
