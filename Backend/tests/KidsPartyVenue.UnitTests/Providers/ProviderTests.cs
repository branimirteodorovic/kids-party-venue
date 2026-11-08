using AwesomeAssertions;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.UnitTests.Providers;

public sealed class ProviderTests
{
    private static readonly Guid CityId = Guid.CreateVersion7();
    private static readonly District Liman = District.Create(CityId, "liman", LocalizedText.Create("Liman").Value).Value;
    private static readonly GeoCoordinate Location = GeoCoordinate.Create(45.2389, 19.8441).Value;
    private static readonly PhoneNumber Phone = PhoneNumber.Create("064 123 4567").Value;
    private static readonly DateTime UtcNow = DateTime.UtcNow;

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
