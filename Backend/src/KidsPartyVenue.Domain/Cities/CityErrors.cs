using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Domain.Cities;

public static class CityErrors
{
    public static readonly Error InvalidSlug = Error.Validation("Cities.InvalidSlug", $"Slug must be lowercase letters and digits separated by hyphens, at most {UrlSlug.MaxLength} characters");
    public static readonly Error UnknownCategory = Error.Validation("Cities.UnknownCategory", "Category is not supported");
    public static readonly Error LocativeNameRequired = Error.Validation("Cities.LocativeNameRequired", "The locative name is required in every language before a category opens");
    public static readonly Error CategoryAlreadyOpen = Error.Conflict("Cities.CategoryAlreadyOpen", "The category is already open in this city");
    public static readonly Error CategoryNotOpen = Error.Conflict("Cities.CategoryNotOpen", "The category is not open in this city");
}
