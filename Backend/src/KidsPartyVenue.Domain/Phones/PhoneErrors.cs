namespace KidsPartyVenue.Domain.Phones;

public static class PhoneErrors
{
    public static readonly Error InvalidNumber = Error.Validation("Phones.InvalidNumber", "Phone number must be a Serbian number or start with a country code");
}
