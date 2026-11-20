using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Lookups;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.CreateProvider;
using KidsPartyVenue.Application.Providers.GetProviderForEditing;
using KidsPartyVenue.Application.Providers.GetPublicProvider;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Venues;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class GetPublicProviderTests(IntegrationTestWebAppFactory factory) : ProviderEditorTestBase(factory)
{
    private readonly ICacheService _cache = factory.Services.GetRequiredService<ICacheService>();

    [Fact]
    public async Task GetPublicProvider_ShouldReturnTheProfile_WhenProviderIsPubliclyVisible()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        List<Guid> amenityIds = await GetAmenityIdsAsync(3);
        await FillInAsync(providerId, amenityIds);
        await SetStatusAsync(providerId, ProviderStatus.Published);
        ProviderForEditingResponse editable = await GetAsAdminAsync(providerId);
        District district = await DbContext.Districts.AsNoTracking().SingleAsync(d => d.Id == editable.DistrictId);

        // Act: no token, the endpoint is anonymous
        HttpResponseMessage response = await GetPublicAsync(editable.Slug);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        string json = await response.Content.ReadAsStringAsync();
        json.Should().NotContainAny("\"status\"", "\"version\"", "\"members\"", "claimedAtUtc", "updatedAtUtc");

        PublicProviderResponse page = (await response.Content.ReadFromJsonAsync<PublicProviderResponse>(JsonOptions))!;
        page.Id.Should().Be(providerId);
        page.Slug.Should().Be(editable.Slug);
        page.Name.Should().Be(editable.Name);
        page.Category.Should().Be(ProviderCategory.Venue);
        page.Description.Should().Be(editable.Description);
        page.Address.Should().Be(editable.Address);
        page.City.Id.Should().Be(editable.CityId);
        page.City.Slug.Should().Be("novi-sad");
        page.District!.Id.Should().Be(district.Id);
        page.District.Slug.Should().Be(district.Slug);
        page.Latitude.Should().BeApproximately(Latitude, 0.000001);
        page.Longitude.Should().BeApproximately(Longitude, 0.000001);
        page.Phone.Should().Be(editable.Phone);
        page.IsClaimed.Should().BeFalse();
        page.BookingPolicy.Should().Be(new BookingPolicyResponse(DepositType.Percentage, 20m, 3, 7));
        page.VenueDetails.Should().Be(editable.VenueDetails);
        page.Amenities.Select(a => a.Slug).Should().Equal("parking", "klima-uredjaj", "wi-fi");
    }

    [Theory]
    [InlineData(ProviderStatus.Draft)]
    [InlineData(ProviderStatus.PendingReview)]
    [InlineData(ProviderStatus.ChangesRequested)]
    public async Task GetPublicProvider_ShouldReturnNotFound_WhenProviderIsNotPublished(ProviderStatus status)
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        await SetStatusAsync(providerId, status);

        // Act
        HttpResponseMessage response = await GetPublicAsync(await GetSlugAsync(providerId));

        // Assert
        await ShouldBeNotFoundAsync(response);
    }

    [Fact]
    public async Task GetPublicProvider_ShouldReturnNotFound_WhenProviderIsSuspended()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        await SetStatusAsync(providerId, ProviderStatus.Published);
        await DbContext.Database.ExecuteSqlAsync($"UPDATE providers SET suspended_at_utc = now(), suspension_reason = 'Test' WHERE id = {providerId}");

        // Act
        HttpResponseMessage response = await GetPublicAsync(await GetSlugAsync(providerId));

        // Assert
        await ShouldBeNotFoundAsync(response);
    }

    [Fact]
    public async Task GetPublicProvider_ShouldReturnNotFound_WhenCategoryIsNotOpenInTheCity()
    {
        // Arrange: venues are open only in Novi Sad
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);
        Result<CreateProviderResponse> created = await SendCommand<CreateProviderCommand, CreateProviderResponse>(
            new CreateProviderCommand(
                UniqueName(), ProviderCategory.Venue, null, Faker.Address.StreetAddress(), cityId, null, 44.8125, 20.4612, null));
        created.IsSuccess.Should().BeTrue();
        await SetStatusAsync(created.Value.Id, ProviderStatus.Published);

        // Act
        HttpResponseMessage response = await GetPublicAsync(await GetSlugAsync(created.Value.Id));

        // Assert
        await ShouldBeNotFoundAsync(response);
    }

    [Fact]
    public async Task GetPublicProvider_ShouldReturnTheSameNotFound_ForAHiddenProviderAndAnUnknownSlug()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();

        // Act
        HttpResponseMessage hidden = await GetPublicAsync(await GetSlugAsync(providerId));
        HttpResponseMessage unknown = await GetPublicAsync($"nepostojeci-{Guid.CreateVersion7():N}");

        // Assert
        ProblemDetails hiddenProblem = await ShouldBeNotFoundAsync(hidden);
        ProblemDetails unknownProblem = await ShouldBeNotFoundAsync(unknown);
        hiddenProblem.Title.Should().Be(unknownProblem.Title);
        hiddenProblem.Status.Should().Be(unknownProblem.Status);
        hiddenProblem.Detail.Should().Be(unknownProblem.Detail);
        hiddenProblem.Type.Should().Be(unknownProblem.Type);
    }

    [Theory]
    [InlineData(null, "Novi Sad", "Novom Sadu", "Klima uređaj")]
    [InlineData("en", "Novi Sad", "Novi Sad", "Air conditioning")]
    [InlineData("ru", "Нови-Сад", "Нови-Саде", "Кондиционер")]
    [InlineData("de", "Novi Sad", "Novom Sadu", "Klima uređaj")]
    public async Task GetPublicProvider_ShouldReturnLookupNames_InTheRequestLanguage(
        string? acceptLanguage, string cityName, string locativeName, string amenityName)
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        await FillInAsync(providerId, await GetAmenityIdsAsync(3));
        await SetStatusAsync(providerId, ProviderStatus.Published);

        // Act
        PublicProviderResponse page = await GetPageAsync(await GetSlugAsync(providerId), acceptLanguage);

        // Assert
        page.City.Name.Should().Be(cityName);
        page.City.LocativeName.Should().Be(locativeName);
        page.Amenities.Select(a => a.Name).Should().Contain(amenityName);
        page.District!.Name.Should().NotBeNullOrEmpty();
        page.Description.Should().Be("Opis");
    }

    [Fact]
    public async Task GetPublicProvider_ShouldReturnNullPolicyAndDetails_WhenTheyAreNotSet()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        await SetStatusAsync(providerId, ProviderStatus.Published);

        // Act
        PublicProviderResponse page = await GetPageAsync(await GetSlugAsync(providerId));

        // Assert
        page.BookingPolicy.Should().BeNull();
        page.VenueDetails.Should().BeNull();
        page.Amenities.Should().BeEmpty();
    }

    [Fact]
    public async Task GetPublicProvider_ShouldServeTheCachedPage_UntilItsTagIsRemoved()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        await SetStatusAsync(providerId, ProviderStatus.Published);
        string slug = await GetSlugAsync(providerId);
        PublicProviderResponse first = await GetPageAsync(slug);
        await DbContext.Database.ExecuteSqlAsync($"UPDATE providers SET name = 'Renamed directly' WHERE id = {providerId}");

        // Act
        PublicProviderResponse cached = await GetPageAsync(slug);
        await _cache.RemoveByTagAsync(LookupsCache.Tag);
        PublicProviderResponse afterLookups = await GetPageAsync(slug);
        await DbContext.Database.ExecuteSqlAsync($"UPDATE providers SET name = 'Renamed again' WHERE id = {providerId}");
        await _cache.RemoveByTagAsync(ProviderCache.Tag(providerId));
        PublicProviderResponse afterProvider = await GetPageAsync(slug);

        // Assert
        cached.Name.Should().Be(first.Name);
        afterLookups.Name.Should().Be("Renamed directly");
        afterProvider.Name.Should().Be("Renamed again");
    }

    [Fact]
    public async Task GetPublicProvider_ShouldShowTheProvider_AsSoonAsItBecomesVisible()
    {
        // Arrange: the first request finds it hidden
        Guid providerId = await CreateProviderAsync();
        string slug = await GetSlugAsync(providerId);
        await ShouldBeNotFoundAsync(await GetPublicAsync(slug));
        await SetStatusAsync(providerId, ProviderStatus.Published);

        // Act
        HttpResponseMessage response = await GetPublicAsync(slug);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetPublicProvider_ShouldReturnFreshData_AfterEachEditorUpdate()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        await SetStatusAsync(providerId, ProviderStatus.Published);
        string slug = await GetSlugAsync(providerId);
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);
        await GetPageAsync(slug, "en");
        await GetPageAsync(slug, "sr");

        // Act and Assert: basic info, read in both cached languages
        string newName = UniqueName();
        uint version = await ReadVersionAsync(await PutAsAdminAsync(providerId, "basic-info", new BasicInfoRequest(
            newName, "Novi opis", current.Address, current.CityId, current.DistrictId, Latitude, Longitude, null, current.Version)));
        (await GetPageAsync(slug, "en")).Name.Should().Be(newName);
        (await GetPageAsync(slug, "sr")).Description.Should().Be("Novi opis");

        // Venue details
        version = await ReadVersionAsync(await PutAsAdminAsync(providerId, "venue-details", VenueDetails(version)));
        (await GetPageAsync(slug)).VenueDetails!.MaxChildren.Should().Be(30);

        // Booking policy
        version = await ReadVersionAsync(await PutAsAdminAsync(
            providerId, "booking-policy", new BookingPolicyRequest(DepositType.None, null, null, 5, version)));
        (await GetPageAsync(slug)).BookingPolicy!.FreeCancellationDays.Should().Be(5);

        // Amenities
        List<Guid> amenityIds = await GetAmenityIdsAsync(1);
        await ReadVersionAsync(await PutAsAdminAsync(providerId, "amenities", new AmenitiesRequest(amenityIds, version)));
        (await GetPageAsync(slug)).Amenities.Select(a => a.Id).Should().Equal(amenityIds);
    }

    [Fact]
    public async Task GetPublicProvider_ShouldKeepTheCachedPage_WhenAnUpdateFails()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        await SetStatusAsync(providerId, ProviderStatus.Published);
        string slug = await GetSlugAsync(providerId);
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);
        PublicProviderResponse first = await GetPageAsync(slug);
        await DbContext.Database.ExecuteSqlAsync($"UPDATE providers SET name = 'Renamed directly' WHERE id = {providerId}");

        // Act: a stale version fails with 409
        HttpResponseMessage update = await PutAsAdminAsync(
            providerId, "booking-policy", new BookingPolicyRequest(DepositType.None, null, null, 5, current.Version));
        PublicProviderResponse page = await GetPageAsync(slug);

        // Assert
        await ShouldFailWith(update, HttpStatusCode.Conflict, ProviderErrors.ConcurrencyConflict.Code);
        page.Name.Should().Be(first.Name);
    }

    [Fact]
    public async Task GetPublicProvider_ShouldFindTheProvider_WhenSlugHasUppercaseLetters()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        await SetStatusAsync(providerId, ProviderStatus.Published);
        string slug = await GetSlugAsync(providerId);

        // Act
        PublicProviderResponse page = await GetPageAsync(slug.ToUpperInvariant());

        // Assert
        page.Id.Should().Be(providerId);
        page.Slug.Should().Be(slug);
    }

    [Theory]
    [InlineData("a_b")]
    [InlineData("a--b")]
    [InlineData("-ab")]
    [InlineData("đak")]
    public async Task GetPublicProvider_ShouldReturnNotFound_WhenSlugIsNotAValidSlug(string slug)
    {
        // Act
        HttpResponseMessage response = await GetPublicAsync(Uri.EscapeDataString(slug));

        // Assert
        await ShouldBeNotFoundAsync(response);
    }

    [Fact]
    public async Task GetPublicProvider_ShouldReturnNotFound_WhenSlugIsTooLong()
    {
        // Act
        HttpResponseMessage response = await GetPublicAsync(new string('a', 101));

        // Assert
        await ShouldBeNotFoundAsync(response);
    }

    private async Task FillInAsync(Guid providerId, List<Guid> amenityIds)
    {
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);
        uint version = await ReadVersionAsync(await PutAsAdminAsync(providerId, "venue-details", VenueDetails(current.Version)));
        version = await ReadVersionAsync(await PutAsAdminAsync(
            providerId, "booking-policy", new BookingPolicyRequest(DepositType.Percentage, 20m, 3, 7, version)));
        await ReadVersionAsync(await PutAsAdminAsync(providerId, "amenities", new AmenitiesRequest(amenityIds, version)));
    }

    // Set directly, so these tests stay about visibility; ProviderReviewWorkflowTests publishes through the API
    private async Task SetStatusAsync(Guid providerId, ProviderStatus status)
    {
        string value = status.ToString();
        await DbContext.Database.ExecuteSqlAsync($"UPDATE providers SET status = {value} WHERE id = {providerId}");
    }

    private Task<string> GetSlugAsync(Guid providerId) =>
        DbContext.Providers.AsNoTracking().Where(p => p.Id == providerId).Select(p => p.Slug).SingleAsync();

    private async Task<HttpResponseMessage> GetPublicAsync(string slug, string? acceptLanguage = null)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, $"providers/{slug}");
        if (acceptLanguage is not null)
        {
            request.Headers.TryAddWithoutValidation("Accept-Language", acceptLanguage);
        }

        return await HttpClient.SendAsync(request);
    }

    private async Task<PublicProviderResponse> GetPageAsync(string slug, string? acceptLanguage = null)
    {
        HttpResponseMessage response = await GetPublicAsync(slug, acceptLanguage);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<PublicProviderResponse>(JsonOptions))!;
    }

    private static async Task<ProblemDetails> ShouldBeNotFoundAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
        ProblemDetails problem = (await response.Content.ReadFromJsonAsync<ProblemDetails>())!;
        problem.Title.Should().Be(ProviderErrors.NotFound.Code);

        return problem;
    }

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
