using System.Net;
using AwesomeAssertions;
using KidsPartyVenue.Application.Providers.GetProviderForEditing;
using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Venues;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class UpdateProviderTests(IntegrationTestWebAppFactory factory) : ProviderEditorTestBase(factory)
{
    [Fact]
    public async Task UpdateBasicInfo_ShouldReplaceBasicInfoAndKeepSlug_WhenRequestIsValid()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);
        District district = await GetNoviSadDistrictAsync();
        string name = UniqueName();

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "basic-info", BasicInfo(
            name, district.CityId, district.Id, before.Version) with { Description = "  ", Phone = null, Address = " Futoška 1 " });

        // Assert
        uint version = await ReadVersionAsync(response);
        ProviderForEditingResponse after = await GetAsAdminAsync(providerId);
        after.Name.Should().Be(name);
        after.Slug.Should().Be(before.Slug);
        after.Description.Should().BeNull();
        after.Phone.Should().BeNull();
        after.Address.Should().Be("Futoška 1");
        after.Version.Should().Be(version).And.NotBe(before.Version);
    }

    [Fact]
    public async Task UpdateBasicInfo_ShouldMoveToCityWithoutDistricts_WhenDistrictIsOmitted()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);
        Guid beogradId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "basic-info", BasicInfo(
            before.Name, beogradId, null, before.Version) with { Latitude = 44.8125, Longitude = 20.4612 });

        // Assert
        await ReadVersionAsync(response);
        ProviderForEditingResponse after = await GetAsAdminAsync(providerId);
        after.CityId.Should().Be(beogradId);
        after.DistrictId.Should().BeNull();
        after.Slug.Should().Be(before.Slug);
    }

    [Fact]
    public async Task UpdateBasicInfo_ShouldReturnBadRequest_WhenDistrictIsMissingInCityWithDistricts()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "basic-info", BasicInfo(
            before.Name, before.CityId, null, before.Version));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.DistrictRequired.Code);
    }

    [Fact]
    public async Task UpdateBasicInfo_ShouldReturnBadRequest_WhenDistrictIsInAnotherCity()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);
        Guid beogradId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "basic-info", BasicInfo(
            before.Name, beogradId, before.DistrictId, before.Version));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.DistrictNotInCity.Code);
    }

    [Fact]
    public async Task UpdateBasicInfo_ShouldReturnNotFound_WhenCityDoesNotExist()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "basic-info", BasicInfo(
            before.Name, Guid.CreateVersion7(), null, before.Version));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, CityErrors.NotFound.Code);
    }

    [Fact]
    public async Task UpdateBasicInfo_ShouldReturnNotFound_WhenDistrictDoesNotExist()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "basic-info", BasicInfo(
            before.Name, before.CityId, Guid.CreateVersion7(), before.Version));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, DistrictErrors.NotFound.Code);
    }

    [Fact]
    public async Task UpdateBasicInfo_ShouldReturnBadRequest_WhenPhoneIsInvalid()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "basic-info", BasicInfo(
            before.Name, before.CityId, before.DistrictId, before.Version) with { Phone = "12" });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, "Phones.InvalidNumber");
    }

    [Fact]
    public async Task UpdateVenueDetails_ShouldSetDetails_WhenRequestIsValid()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "venue-details", VenueDetails(before.Version));

        // Assert
        await ReadVersionAsync(response);
        ProviderForEditingResponse after = await GetAsAdminAsync(providerId);
        after.VenueDetails.Should().Be(new VenueDetailsResponse(
            30, null, 3, 10, OutsideFoodRule.NotAllowed, OutsideFoodRule.AllowedWithFee, FoodService.Included,
            HasOwnAnimator: true, HasOwnPhotographer: false, HasSmokingArea: true));
    }

    [Fact]
    public async Task UpdateVenueDetails_ShouldReturnBadRequest_WhenAgeRangeIsInverted()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(
            providerId, "venue-details", VenueDetails(before.Version) with { MinAge = 8, MaxAge = 6 });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, VenueDetailsErrors.InvalidAgeRange.Code);
    }

    [Fact]
    public async Task UpdateBookingPolicy_ShouldSetPolicy_WhenRequestIsValid()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(
            providerId, "booking-policy", new BookingPolicyRequest(DepositType.Percentage, 30, 3, 7, before.Version));

        // Assert
        await ReadVersionAsync(response);
        ProviderForEditingResponse after = await GetAsAdminAsync(providerId);
        after.BookingPolicy.Should().Be(new BookingPolicyResponse(DepositType.Percentage, 30, 3, 7));
    }

    [Fact]
    public async Task UpdateBookingPolicy_ShouldReturnBadRequest_WhenDepositTermsAreGivenWithoutDeposit()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(
            providerId, "booking-policy", new BookingPolicyRequest(DepositType.None, 30, null, 7, before.Version));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, BookingPolicyErrors.DepositTermsWithoutDeposit.Code);
    }

    [Fact]
    public async Task UpdateAmenities_ShouldReplaceSet_WhenAmenitiesExist()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        List<Guid> amenityIds = await GetAmenityIdsAsync(3);
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);
        uint version = await ReadVersionAsync(await PutAsAdminAsync(
            providerId, "amenities", new AmenitiesRequest([amenityIds[0], amenityIds[1]], before.Version)));

        // Act: one kept, one removed, one added, and a duplicate
        HttpResponseMessage response = await PutAsAdminAsync(
            providerId, "amenities", new AmenitiesRequest([amenityIds[1], amenityIds[2], amenityIds[2]], version));

        // Assert
        await ReadVersionAsync(response);
        ProviderForEditingResponse after = await GetAsAdminAsync(providerId);
        after.AmenityIds.Should().BeEquivalentTo([amenityIds[1], amenityIds[2]]);
    }

    [Fact]
    public async Task UpdateAmenities_ShouldClearSet_WhenListIsEmpty()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        List<Guid> amenityIds = await GetAmenityIdsAsync(2);
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);
        uint version = await ReadVersionAsync(await PutAsAdminAsync(
            providerId, "amenities", new AmenitiesRequest(amenityIds, before.Version)));

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "amenities", new AmenitiesRequest([], version));

        // Assert
        await ReadVersionAsync(response);
        (await GetAsAdminAsync(providerId)).AmenityIds.Should().BeEmpty();
    }

    [Fact]
    public async Task UpdateAmenities_ShouldReturnSameVersion_WhenSetIsUnchanged()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "amenities", new AmenitiesRequest([], before.Version));

        // Assert
        (await ReadVersionAsync(response)).Should().Be(before.Version);
    }

    [Fact]
    public async Task UpdateAmenities_ShouldReturnNotFoundAndChangeNothing_WhenAmenityDoesNotExist()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        List<Guid> amenityIds = await GetAmenityIdsAsync(1);
        ProviderForEditingResponse before = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(
            providerId, "amenities", new AmenitiesRequest([amenityIds[0], Guid.CreateVersion7()], before.Version));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, AmenityErrors.NotFound.Code);
        ProviderForEditingResponse after = await GetAsAdminAsync(providerId);
        after.AmenityIds.Should().BeEmpty();
        after.Version.Should().Be(before.Version);
    }

    [Fact]
    public async Task Update_ShouldReturnConflictAndChangeNothing_WhenVersionIsStale()
    {
        // Arrange: an admin saves the policy, then another edit arrives with the version read before that save
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse stale = await GetAsAdminAsync(providerId);
        await ReadVersionAsync(await PutAsAdminAsync(
            providerId, "booking-policy", new BookingPolicyRequest(DepositType.None, null, null, 7, stale.Version)));

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, "basic-info", BasicInfo(
            UniqueName(), stale.CityId, stale.DistrictId, stale.Version));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.Conflict, ProviderErrors.ConcurrencyConflict.Code);
        (await GetAsAdminAsync(providerId)).Name.Should().Be(stale.Name);
    }

    [Theory]
    [InlineData("basic-info")]
    [InlineData("venue-details")]
    [InlineData("booking-policy")]
    [InlineData("amenities")]
    public async Task Update_ShouldReturnConflict_WhenVersionIsStaleOnAnySection(string section)
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(providerId, section, ValidRequest(section, current, current.Version + 1));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.Conflict, ProviderErrors.ConcurrencyConflict.Code);
    }

    [Fact]
    public async Task Update_ShouldSucceedTwiceInARow_WhenReturnedVersionIsUsed()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);
        uint version = await ReadVersionAsync(await PutAsAdminAsync(providerId, "venue-details", VenueDetails(current.Version)));

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(
            providerId, "booking-policy", new BookingPolicyRequest(DepositType.FixedAmount, 5000, 5, 14, version));

        // Assert
        uint next = await ReadVersionAsync(response);
        ProviderForEditingResponse after = await GetAsAdminAsync(providerId);
        after.Version.Should().Be(next);
        after.VenueDetails.Should().NotBeNull();
        after.BookingPolicy!.DepositType.Should().Be(DepositType.FixedAmount);
    }

    [Theory]
    [InlineData("basic-info")]
    [InlineData("venue-details")]
    [InlineData("booking-policy")]
    [InlineData("amenities")]
    public async Task Update_ShouldReturnNotFound_WhenProviderDoesNotExist(string section)
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await PutAsAdminAsync(
            Guid.CreateVersion7(), section, ValidRequest(section, current, current.Version));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, ProviderErrors.NotFound.Code);
    }

    [Theory]
    [InlineData("basic-info")]
    [InlineData("venue-details")]
    [InlineData("booking-policy")]
    [InlineData("amenities")]
    public async Task Update_ShouldReturnUnauthorized_WhenTokenIsMissing(string section)
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await SendAsync(
            HttpMethod.Put, $"providers/{providerId}/{section}", ValidRequest(section, current, current.Version), accessToken: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("basic-info")]
    [InlineData("venue-details")]
    [InlineData("booking-policy")]
    [InlineData("amenities")]
    public async Task Update_ShouldReturnForbidden_WhenUserIsParent(string section)
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);

        // Act
        HttpResponseMessage response = await SendAsync(
            HttpMethod.Put, $"providers/{providerId}/{section}", ValidRequest(section, current, current.Version), await ParentTokenAsync());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static object ValidRequest(string section, ProviderForEditingResponse current, uint version) =>
        section switch
        {
            "basic-info" => BasicInfo(current.Name, current.CityId, current.DistrictId, version),
            "venue-details" => VenueDetails(version),
            "booking-policy" => new BookingPolicyRequest(DepositType.None, null, null, 7, version),
            "amenities" => new AmenitiesRequest([], version),
            _ => throw new ArgumentOutOfRangeException(nameof(section))
        };

    private static BasicInfoRequest BasicInfo(string name, Guid cityId, Guid? districtId, uint version) =>
        new(name, "Opis", Faker.Address.StreetAddress(), cityId, districtId, Latitude, Longitude, "063 123 4567", version);

    private static VenueDetailsRequest VenueDetails(uint version) =>
        new(30, null, 3, 10, OutsideFoodRule.NotAllowed, OutsideFoodRule.AllowedWithFee, FoodService.Included,
            HasOwnAnimator: true, HasOwnPhotographer: false, HasSmokingArea: true, version);

    private sealed record BasicInfoRequest(
        string Name,
        string? Description,
        string Address,
        Guid CityId,
        Guid? DistrictId,
        double Latitude,
        double Longitude,
        string? Phone,
        uint Version);

    private sealed record VenueDetailsRequest(
        int MaxChildren,
        int? MaxAdults,
        int MinAge,
        int MaxAge,
        OutsideFoodRule OutsideCatering,
        OutsideFoodRule OutsideCake,
        FoodService FoodService,
        bool HasOwnAnimator,
        bool HasOwnPhotographer,
        bool HasSmokingArea,
        uint Version);

    private sealed record BookingPolicyRequest(
        DepositType DepositType,
        decimal? DepositValue,
        int? DepositDueDays,
        int FreeCancellationDays,
        uint Version);

    private sealed record AmenitiesRequest(List<Guid> AmenityIds, uint Version);
}
