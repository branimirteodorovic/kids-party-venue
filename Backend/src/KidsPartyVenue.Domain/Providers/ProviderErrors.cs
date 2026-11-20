using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Domain.Providers;

public static class ProviderErrors
{
    public static readonly Error NotFound = Error.NotFound("Providers.NotFound", "Provider is not found");
    public static readonly Error ConcurrencyConflict = Error.Conflict("Providers.ConcurrencyConflict", "The provider was changed by someone else; load it again and repeat the change");
    public static readonly Error InvalidSlug = Error.Validation("Providers.InvalidSlug", $"Slug must be lowercase letters and digits separated by hyphens, at most {UrlSlug.MaxLength} characters");
    public static readonly Error ReservedSlug = Error.Validation("Providers.ReservedSlug", "Slug is a word the public URLs use for something else");
    public static readonly Error NameRequired = Error.Validation("Providers.NameRequired", "Name is required");
    public static readonly Error NameTooLong = Error.Validation("Providers.NameTooLong", $"Name can't be longer than {Provider.NameMaxLength} characters");
    public static readonly Error UnknownCategory = Error.Validation("Providers.UnknownCategory", "Category is not supported");
    public static readonly Error DescriptionTooLong = Error.Validation("Providers.DescriptionTooLong", $"Description can't be longer than {Provider.DescriptionMaxLength} characters");
    public static readonly Error AddressRequired = Error.Validation("Providers.AddressRequired", "Address is required");
    public static readonly Error AddressTooLong = Error.Validation("Providers.AddressTooLong", $"Address can't be longer than {Provider.AddressMaxLength} characters");
    public static readonly Error DistrictNotInCity = Error.Validation("Providers.DistrictNotInCity", "The district is not in the provider's city");
    public static readonly Error DistrictRequired = Error.Validation("Providers.DistrictRequired", "A district is required in a city that has districts");
    public static readonly Error LocationOutsideSerbia = Error.Validation("Providers.LocationOutsideSerbia", "Location must be in Serbia");
    public static readonly Error VenueDetailsNotForCategory = Error.Validation("Providers.VenueDetailsNotForCategory", "Only venues have venue details");
    public static readonly Error AmenitiesNotForCategory = Error.Validation("Providers.AmenitiesNotForCategory", "Only venues have amenities");
    public static readonly Error UnknownMemberRole = Error.Validation("Providers.UnknownMemberRole", "Member role is not supported");
    public static readonly Error AlreadyMember = Error.Conflict("Providers.AlreadyMember", "The user is already a member of the provider");
    public static readonly Error MemberNotFound = Error.NotFound("Providers.MemberNotFound", "The user is not a member of the provider");
    public static readonly Error LastOwner = Error.Conflict("Providers.LastOwner", "The provider's only owner can't be removed");
}
