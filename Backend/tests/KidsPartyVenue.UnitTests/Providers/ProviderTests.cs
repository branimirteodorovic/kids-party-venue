using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class ProviderTests
{
    private static readonly Guid CityId = Guid.CreateVersion7();
    private static readonly District Liman = District.Create(CityId, "liman", LocalizedText.Create("Liman").Value).Value;
    private static readonly GeoCoordinate Location = GeoCoordinate.Create(45.2389, 19.8441).Value;
    private static readonly PhoneNumber Phone = PhoneNumber.Create("064 123 4567").Value;
    private static readonly DateTime UtcNow = DateTime.UtcNow;
    private static readonly Amenity Parking = CreateAmenity("parking");
    private static readonly Amenity Trampoline = CreateAmenity("trampolina");
    private static readonly Amenity BallPit = CreateAmenity("bazen-sa-lopticama");

    [Fact]
    public void Create_ShouldReturnDraft_WhenInputIsValid()
    {
        // Act
        Result<Provider> result = Provider.Create(
            "igraonica-zeka", "  Igraonica Zeka ", ProviderCategory.Venue, " Rođendani za decu ", " Bulevar oslobođenja 1 ",
            CityId, Liman, cityHasDistricts: true, Location, Phone, isClaimed: false, UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        Provider provider = result.Value;
        provider.Id.Should().NotBeEmpty();
        provider.Slug.Should().Be("igraonica-zeka");
        provider.Name.Should().Be("Igraonica Zeka");
        provider.Category.Should().Be(ProviderCategory.Venue);
        provider.Description.Should().Be("Rođendani za decu");
        provider.Address.Should().Be("Bulevar oslobođenja 1");
        provider.CityId.Should().Be(CityId);
        provider.DistrictId.Should().Be(Liman.Id);
        provider.Location.Should().Be(Location);
        provider.Phone.Should().Be(Phone);
        provider.Status.Should().Be(ProviderStatus.Draft);
        provider.ClaimedAtUtc.Should().BeNull();
        provider.IsClaimed.Should().BeFalse();
        provider.SuspendedAtUtc.Should().BeNull();
        provider.CreatedAtUtc.Should().Be(UtcNow);
        provider.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void Create_ShouldSetClaimedAt_WhenCreatedAsClaimed()
    {
        // Act
        Result<Provider> result = Create(isClaimed: true);

        // Assert
        result.Value.ClaimedAtUtc.Should().Be(UtcNow);
        result.Value.IsClaimed.Should().BeTrue();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldStoreNoDescription_WhenItIsBlank(string? description)
    {
        // Act
        Result<Provider> result = Create(description: description);

        // Assert
        result.Value.Description.Should().BeNull();
    }

    [Fact]
    public void Create_ShouldStoreNoPhone_WhenItIsMissing()
    {
        // Act
        Result<Provider> result = Create(phone: null);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Phone.Should().BeNull();
    }

    [Theory]
    [InlineData("")]
    [InlineData("Igraonica-Zeka")]
    [InlineData("igraonica zeka")]
    [InlineData("igraonica--zeka")]
    [InlineData("čigra")]
    [InlineData(null)]
    public void Create_ShouldFail_WhenSlugIsInvalid(string? slug)
    {
        // Act
        Result<Provider> result = Create(slug: slug);

        // Assert
        result.Error.Should().Be(ProviderErrors.InvalidSlug);
    }

    [Fact]
    public void Create_ShouldFail_WhenSlugIsReserved()
    {
        // Act
        Result<Provider> result = Create(slug: "kvart");

        // Assert
        result.Error.Should().Be(ProviderErrors.ReservedSlug);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenNameIsBlank(string? name)
    {
        // Act
        Result<Provider> result = Create(name: name);

        // Assert
        result.Error.Should().Be(ProviderErrors.NameRequired);
    }

    [Fact]
    public void Create_ShouldAcceptAName_AtItsMaxLength()
    {
        // Act
        Result<Provider> result = Create(name: new string('a', Provider.NameMaxLength));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldFail_WhenNameIsTooLong()
    {
        // Act
        Result<Provider> result = Create(name: new string('a', Provider.NameMaxLength + 1));

        // Assert
        result.Error.Should().Be(ProviderErrors.NameTooLong);
    }

    [Fact]
    public void Create_ShouldFail_WhenCategoryIsUnknown()
    {
        // Act
        Result<Provider> result = Create(category: (ProviderCategory)99);

        // Assert
        result.Error.Should().Be(ProviderErrors.UnknownCategory);
    }

    [Fact]
    public void Create_ShouldAcceptADescription_AtItsMaxLength()
    {
        // Act
        Result<Provider> result = Create(description: new string('a', Provider.DescriptionMaxLength));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldFail_WhenDescriptionIsTooLong()
    {
        // Act
        Result<Provider> result = Create(description: new string('a', Provider.DescriptionMaxLength + 1));

        // Assert
        result.Error.Should().Be(ProviderErrors.DescriptionTooLong);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Create_ShouldFail_WhenAddressIsBlank(string? address)
    {
        // Act
        Result<Provider> result = Create(address: address);

        // Assert
        result.Error.Should().Be(ProviderErrors.AddressRequired);
    }

    [Fact]
    public void Create_ShouldAcceptAnAddress_AtItsMaxLength()
    {
        // Act
        Result<Provider> result = Create(address: new string('a', Provider.AddressMaxLength));

        // Assert
        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public void Create_ShouldFail_WhenAddressIsTooLong()
    {
        // Act
        Result<Provider> result = Create(address: new string('a', Provider.AddressMaxLength + 1));

        // Assert
        result.Error.Should().Be(ProviderErrors.AddressTooLong);
    }

    [Fact]
    public void Create_ShouldFail_WhenDistrictIsInAnotherCity()
    {
        // Arrange
        District otherCityDistrict = District.Create(Guid.CreateVersion7(), "centar", LocalizedText.Create("Centar").Value).Value;

        // Act
        Result<Provider> result = Create(district: otherCityDistrict, cityHasDistricts: true);

        // Assert
        result.Error.Should().Be(ProviderErrors.DistrictNotInCity);
    }

    [Fact]
    public void Create_ShouldFail_WhenCityHasDistrictsAndNoneIsGiven()
    {
        // Act
        Result<Provider> result = Create(district: null, cityHasDistricts: true);

        // Assert
        result.Error.Should().Be(ProviderErrors.DistrictRequired);
    }

    [Fact]
    public void Create_ShouldSucceedWithoutDistrict_WhenCityHasNone()
    {
        // Act
        Result<Provider> result = Create(district: null, cityHasDistricts: false);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.DistrictId.Should().BeNull();
    }

    [Theory]
    [InlineData(19.8441, 45.2389)]
    [InlineData(48.2082, 16.3738)]
    [InlineData(41.7, 21.0)]
    [InlineData(44.0, 23.2)]
    public void Create_ShouldFail_WhenLocationIsOutsideSerbia(double latitude, double longitude)
    {
        // Act
        Result<Provider> result = Create(location: GeoCoordinate.Create(latitude, longitude).Value);

        // Assert
        result.Error.Should().Be(ProviderErrors.LocationOutsideSerbia);
    }

    [Fact]
    public void Create_ShouldLeaveBookingPolicyEmpty()
    {
        // Act
        Result<Provider> result = Create();

        // Assert
        result.Value.BookingPolicy.Should().BeNull();
    }

    [Fact]
    public void ChangeBookingPolicy_ShouldSetPolicyAndUpdatedAt()
    {
        // Arrange
        Provider provider = Create().Value;
        BookingPolicy policy = BookingPolicy.Create(DepositType.Percentage, 30, 3, 7).Value;
        DateTime later = UtcNow.AddMinutes(5);

        // Act
        provider.ChangeBookingPolicy(policy, later);

        // Assert
        provider.BookingPolicy.Should().Be(policy);
        provider.UpdatedAtUtc.Should().Be(later);
        provider.CreatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void ChangeBookingPolicy_ShouldThrow_WhenPolicyIsNull()
    {
        // Arrange
        Provider provider = Create().Value;

        // Act
        Action act = () => provider.ChangeBookingPolicy(null!, UtcNow);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldLeaveVenueDetailsEmpty()
    {
        // Act
        Result<Provider> result = Create();

        // Assert
        result.Value.VenueDetails.Should().BeNull();
    }

    [Fact]
    public void ChangeVenueDetails_ShouldSetDetailsAndUpdatedAt_WhenProviderIsAVenue()
    {
        // Arrange
        Provider provider = Create().Value;
        VenueDetails details = CreateVenueDetails();
        DateTime later = UtcNow.AddMinutes(5);

        // Act
        Result result = provider.ChangeVenueDetails(details, later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.VenueDetails.Should().Be(details);
        provider.UpdatedAtUtc.Should().Be(later);
        provider.CreatedAtUtc.Should().Be(UtcNow);
    }

    [Theory]
    [InlineData(ProviderCategory.Catering)]
    [InlineData(ProviderCategory.Cakes)]
    public void ChangeVenueDetails_ShouldFail_WhenProviderIsNotAVenue(ProviderCategory category)
    {
        // Arrange
        Provider provider = Create(category: category).Value;

        // Act
        Result result = provider.ChangeVenueDetails(CreateVenueDetails(), UtcNow.AddMinutes(5));

        // Assert
        result.Error.Should().Be(ProviderErrors.VenueDetailsNotForCategory);
        provider.VenueDetails.Should().BeNull();
        provider.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void ChangeVenueDetails_ShouldThrow_WhenDetailsAreNull()
    {
        // Arrange
        Provider provider = Create().Value;

        // Act
        Action act = () => provider.ChangeVenueDetails(null!, UtcNow);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Create_ShouldLeaveAmenitiesEmpty()
    {
        // Act
        Result<Provider> result = Create();

        // Assert
        result.Value.Amenities.Should().BeEmpty();
    }

    [Fact]
    public void ChangeAmenities_ShouldSetAmenitiesAndUpdatedAt_WhenProviderIsAVenue()
    {
        // Arrange
        Provider provider = Create().Value;
        DateTime later = UtcNow.AddMinutes(5);

        // Act
        Result result = provider.ChangeAmenities([Parking, Trampoline], later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Amenities.Select(a => a.AmenityId).Should().BeEquivalentTo([Parking.Id, Trampoline.Id]);
        provider.Amenities.Should().AllSatisfy(a => a.ProviderId.Should().Be(provider.Id));
        provider.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void ChangeAmenities_ShouldAddRemoveAndKeep_WhenSetChanges()
    {
        // Arrange
        Provider provider = Create().Value;
        provider.ChangeAmenities([Parking, Trampoline], UtcNow);
        ProviderAmenity kept = provider.Amenities.Single(a => a.AmenityId == Parking.Id);

        // Act
        provider.ChangeAmenities([Parking, BallPit], UtcNow.AddMinutes(5));

        // Assert
        provider.Amenities.Select(a => a.AmenityId).Should().BeEquivalentTo([Parking.Id, BallPit.Id]);
        provider.Amenities.Should().Contain(a => ReferenceEquals(a, kept));
    }

    [Fact]
    public void ChangeAmenities_ShouldLeaveUpdatedAt_WhenSetIsTheSame()
    {
        // Arrange
        Provider provider = Create().Value;
        DateTime first = UtcNow.AddMinutes(5);
        provider.ChangeAmenities([Parking, Trampoline], first);

        // Act
        Result result = provider.ChangeAmenities([Trampoline, Parking], first.AddMinutes(5));

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.UpdatedAtUtc.Should().Be(first);
    }

    [Fact]
    public void ChangeAmenities_ShouldClearAmenities_WhenCollectionIsEmpty()
    {
        // Arrange
        Provider provider = Create().Value;
        provider.ChangeAmenities([Parking], UtcNow);
        DateTime later = UtcNow.AddMinutes(5);

        // Act
        provider.ChangeAmenities([], later);

        // Assert
        provider.Amenities.Should().BeEmpty();
        provider.UpdatedAtUtc.Should().Be(later);
    }

    [Fact]
    public void ChangeAmenities_ShouldStoreOnce_WhenAmenityIsGivenTwice()
    {
        // Arrange
        Provider provider = Create().Value;

        // Act
        provider.ChangeAmenities([Parking, Parking], UtcNow);

        // Assert
        provider.Amenities.Should().ContainSingle().Which.AmenityId.Should().Be(Parking.Id);
    }

    [Theory]
    [InlineData(ProviderCategory.Catering)]
    [InlineData(ProviderCategory.Cakes)]
    public void ChangeAmenities_ShouldFail_WhenProviderIsNotAVenue(ProviderCategory category)
    {
        // Arrange
        Provider provider = Create(category: category).Value;

        // Act
        Result result = provider.ChangeAmenities([Parking], UtcNow.AddMinutes(5));

        // Assert
        result.Error.Should().Be(ProviderErrors.AmenitiesNotForCategory);
        provider.Amenities.Should().BeEmpty();
        provider.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void ChangeAmenities_ShouldThrow_WhenAmenitiesAreNull()
    {
        // Arrange
        Provider provider = Create().Value;

        // Act
        Action act = () => provider.ChangeAmenities(null!, UtcNow);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ChangeBasicInfo_ShouldReplaceBasicInfo_WhenInputIsValid()
    {
        // Arrange
        Provider provider = Create(district: Liman, cityHasDistricts: true).Value;
        Guid otherCityId = Guid.CreateVersion7();
        GeoCoordinate otherLocation = GeoCoordinate.Create(44.8125, 20.4612).Value;
        DateTime later = UtcNow.AddMinutes(5);

        // Act
        Result result = provider.ChangeBasicInfo(
            "  Kockica ", " Igraonica ", " Knez Mihailova 5 ", otherCityId, null, cityHasDistricts: false,
            otherLocation, Phone, later);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Name.Should().Be("Kockica");
        provider.Description.Should().Be("Igraonica");
        provider.Address.Should().Be("Knez Mihailova 5");
        provider.CityId.Should().Be(otherCityId);
        provider.DistrictId.Should().BeNull();
        provider.Location.Should().Be(otherLocation);
        provider.Phone.Should().Be(Phone);
        provider.UpdatedAtUtc.Should().Be(later);
        provider.CreatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void ChangeBasicInfo_ShouldKeepSlugCategoryAndStatus_WhenNameChanges()
    {
        // Arrange
        Provider provider = Create().Value;

        // Act
        Result result = provider.ChangeBasicInfo(
            "Sasvim drugo ime", null, "Bulevar oslobođenja 1", CityId, null, cityHasDistricts: false, Location, null, UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Slug.Should().Be("igraonica-zeka");
        provider.Category.Should().Be(ProviderCategory.Venue);
        provider.Status.Should().Be(ProviderStatus.Draft);
        provider.IsClaimed.Should().BeFalse();
    }

    [Theory]
    [InlineData(null)]
    [InlineData("   ")]
    public void ChangeBasicInfo_ShouldClearDescriptionAndPhone_WhenTheyAreBlank(string? description)
    {
        // Arrange
        Provider provider = Create(phone: Phone).Value;

        // Act
        Result result = provider.ChangeBasicInfo(
            "Igraonica Zeka", description, "Bulevar oslobođenja 1", CityId, null, cityHasDistricts: false, Location, null, UtcNow);

        // Assert
        result.IsSuccess.Should().BeTrue();
        provider.Description.Should().BeNull();
        provider.Phone.Should().BeNull();
    }

    public static TheoryData<string, string?, string, bool, double, Error> InvalidBasicInfo => new()
    {
        { "  ", null, "Adresa 1", false, 45.2389, ProviderErrors.NameRequired },
        { new string('a', Provider.NameMaxLength + 1), null, "Adresa 1", false, 45.2389, ProviderErrors.NameTooLong },
        { "Zeka", new string('a', Provider.DescriptionMaxLength + 1), "Adresa 1", false, 45.2389, ProviderErrors.DescriptionTooLong },
        { "Zeka", null, " ", false, 45.2389, ProviderErrors.AddressRequired },
        { "Zeka", null, new string('a', Provider.AddressMaxLength + 1), false, 45.2389, ProviderErrors.AddressTooLong },
        { "Zeka", null, "Adresa 1", true, 45.2389, ProviderErrors.DistrictRequired },
        { "Zeka", null, "Adresa 1", false, 47.4979, ProviderErrors.LocationOutsideSerbia }
    };

    [Theory]
    [MemberData(nameof(InvalidBasicInfo))]
    public void ChangeBasicInfo_ShouldFailAndChangeNothing_WhenInputIsInvalid(
        string name, string? description, string address, bool cityHasDistricts, double latitude, Error expected)
    {
        // Arrange
        Provider provider = Create().Value;
        GeoCoordinate location = GeoCoordinate.Create(latitude, 19.8441).Value;

        // Act
        Result result = provider.ChangeBasicInfo(
            name, description, address, CityId, null, cityHasDistricts, location, null, UtcNow.AddMinutes(5));

        // Assert
        result.Error.Should().Be(expected);
        provider.Name.Should().Be("Igraonica Zeka");
        provider.Address.Should().Be("Bulevar oslobođenja 1");
        provider.UpdatedAtUtc.Should().Be(UtcNow);
    }

    [Fact]
    public void ChangeBasicInfo_ShouldFail_WhenDistrictIsInAnotherCity()
    {
        // Arrange
        Provider provider = Create().Value;

        // Act
        Result result = provider.ChangeBasicInfo(
            "Igraonica Zeka", null, "Bulevar oslobođenja 1", Guid.CreateVersion7(), Liman, cityHasDistricts: true, Location, null, UtcNow);

        // Assert
        result.Error.Should().Be(ProviderErrors.DistrictNotInCity);
        provider.CityId.Should().Be(CityId);
    }

    private static Amenity CreateAmenity(string slug) =>
        Amenity.Create(slug, LocalizedText.Create(slug).Value, 1).Value;

    private static VenueDetails CreateVenueDetails() =>
        VenueDetails.Create(
            30, null, 3, 10, OutsideFoodRule.NotAllowed, OutsideFoodRule.Allowed, FoodService.ForPurchase,
            hasOwnAnimator: true, hasOwnPhotographer: false, hasSmokingArea: false).Value;

    private static Result<Provider> Create(
        string? slug = "igraonica-zeka",
        string? name = "Igraonica Zeka",
        ProviderCategory category = ProviderCategory.Venue,
        string? description = "Rođendani za decu",
        string? address = "Bulevar oslobođenja 1",
        District? district = null,
        bool cityHasDistricts = false,
        GeoCoordinate? location = null,
        PhoneNumber? phone = null,
        bool isClaimed = false) =>
        Provider.Create(
            slug!, name!, category, description, address!, CityId, district, cityHasDistricts,
            location ?? Location, phone, isClaimed, UtcNow);
}
