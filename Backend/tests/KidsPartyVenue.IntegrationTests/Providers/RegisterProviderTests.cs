using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Application.Providers.GetProviderForEditing;
using KidsPartyVenue.Application.Providers.RegisterProvider;
using KidsPartyVenue.Application.Users.GetCurrentUser;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using KidsPartyVenue.IntegrationTests.Users;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class RegisterProviderTests(IntegrationTestWebAppFactory factory) : ProviderEditorTestBase(factory)
{
    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task RegisterProvider_ShouldCreateClaimedDraftOwnedByCaller_WhenRequestIsValid()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);
        string name = UniqueName();
        DateTime before = DateTime.UtcNow;

        // Act
        HttpResponseMessage response = await RegisterAsync(
            NewRequest(name, district.CityId, district.Id) with { Phone = "063 123 4567" }, user.AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RegisterProviderResponse body = (await response.Content.ReadFromJsonAsync<RegisterProviderResponse>())!;
        response.Headers.Location!.OriginalString.Should().Be($"/providers/{body.Id}");

        Provider saved = await DbContext.Providers.AsNoTracking()
            .Include(p => p.Members)
            .Include(p => p.Amenities)
            .SingleAsync(p => p.Id == body.Id);
        saved.Slug.Should().Be(body.Slug);
        saved.Name.Should().Be(name);
        saved.DistrictId.Should().Be(district.Id);
        saved.Phone!.Value.Should().Be("+381631234567");
        saved.Status.Should().Be(ProviderStatus.Draft);
        saved.CreatedAtUtc.Should().BeOnOrAfter(before.AddSeconds(-1));
        saved.ClaimedAtUtc.Should().Be(saved.CreatedAtUtc);
        saved.UpdatedAtUtc.Should().Be(saved.CreatedAtUtc);
        saved.BookingPolicy.Should().BeNull();
        saved.VenueDetails.Should().BeNull();
        saved.Amenities.Should().BeEmpty();

        ProviderMember member = saved.Members.Should().ContainSingle().Subject;
        member.UserId.Should().Be(user.UserId);
        member.Role.Should().Be(ProviderMemberRole.Owner);
        member.JoinedAtUtc.Should().Be(saved.CreatedAtUtc);
    }

    [Fact]
    public async Task RegisterProvider_ShouldAddProviderMemberRoleOnce_WhenUserRegistersTwice()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);

        // Act
        HttpResponseMessage first = await RegisterAsync(NewRequest(UniqueName(), cityId), user.AccessToken);
        List<string> rolesAfterFirst = await GetRolesAsync(user.AccessToken);
        HttpResponseMessage second = await RegisterAsync(NewRequest(UniqueName(), cityId), user.AccessToken);

        // Assert
        first.StatusCode.Should().Be(HttpStatusCode.Created);
        second.StatusCode.Should().Be(HttpStatusCode.Created);
        rolesAfterFirst.Should().BeEquivalentTo([Role.Parent, Role.ProviderMember]);
        (await GetRolesAsync(user.AccessToken)).Should().BeEquivalentTo([Role.Parent, Role.ProviderMember]);
    }

    [Fact]
    public async Task RegisterProvider_ShouldOpenEditorToSameToken_WhenMembershipsWereCached()
    {
        // Arrange: a denied editor request caches the user's empty memberships
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);
        HttpResponseMessage denied = await SendAsync(HttpMethod.Get, $"providers/{Guid.CreateVersion7()}", null, user.AccessToken);
        denied.StatusCode.Should().Be(HttpStatusCode.Forbidden);

        // Act
        HttpResponseMessage response = await RegisterAsync(NewRequest(UniqueName(), cityId), user.AccessToken);

        // Assert
        RegisterProviderResponse body = (await response.Content.ReadFromJsonAsync<RegisterProviderResponse>())!;
        HttpResponseMessage get = await SendAsync(HttpMethod.Get, $"providers/{body.Id}", null, user.AccessToken);
        get.StatusCode.Should().Be(HttpStatusCode.OK);
        ProviderForEditingResponse current = (await get.Content.ReadFromJsonAsync<ProviderForEditingResponse>(JsonOptions))!;

        List<Guid> amenityIds = await GetAmenityIdsAsync(1);
        HttpResponseMessage put = await SendAsync(
            HttpMethod.Put, $"providers/{body.Id}/amenities", new { amenityIds, current.Version }, user.AccessToken);
        await ReadVersionAsync(put);
    }

    [Fact]
    public async Task RegisterProvider_ShouldSaveNothing_WhenProviderInsertFails()
    {
        // Arrange: the role is saved before the provider, inside the same transaction
        WebApplicationFactory<Program> failingFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IProviderRepository, ThrowingProviderRepository>()));
        using HttpClient client = failingFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost/api/")
        });
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);
        // That host signs tokens with its own key, so the user logs in there
        LoggedInUser user = await client.RegisterAndLoginAsync(Faker);
        string name = UniqueName();

        // Act
        HttpResponseMessage response = await client.SendAsync(NewMessage(NewRequest(name, cityId), user.AccessToken));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await DbContext.Providers.AnyAsync(p => p.Name == name)).Should().BeFalse();
        List<string> roles = await DbContext.UserRoles
            .Where(ur => ur.UserId == user.UserId)
            .Join(DbContext.Roles, ur => ur.RoleId, r => r.Id, (_, r) => r.Name!)
            .ToListAsync();
        roles.Should().BeEquivalentTo([Role.Parent]);
    }

    [Fact]
    public async Task RegisterProvider_ShouldCreateWithoutDistrict_WhenCityIsHiddenAndHasNoDistricts()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await RegisterAsync(NewRequest(UniqueName(), cityId), await ParentTokenAsync());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        RegisterProviderResponse body = (await response.Content.ReadFromJsonAsync<RegisterProviderResponse>())!;
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == body.Id);
        saved.DistrictId.Should().BeNull();
        saved.Phone.Should().BeNull();
    }

    [Fact]
    public async Task RegisterProvider_ShouldReturnBadRequest_WhenDistrictIsMissingInCityWithDistricts()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();

        // Act
        HttpResponseMessage response = await RegisterAsync(NewRequest(UniqueName(), district.CityId), await ParentTokenAsync());

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.DistrictRequired.Code);
    }

    [Fact]
    public async Task RegisterProvider_ShouldReturnBadRequest_WhenDistrictIsInAnotherCity()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();
        Guid otherCityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await RegisterAsync(
            NewRequest(UniqueName(), otherCityId, district.Id), await ParentTokenAsync());

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.DistrictNotInCity.Code);
    }

    [Fact]
    public async Task RegisterProvider_ShouldReturnNotFoundAndSaveNothing_WhenCityDoesNotExist()
    {
        // Arrange
        string name = UniqueName();
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);

        // Act
        HttpResponseMessage response = await RegisterAsync(NewRequest(name, Guid.CreateVersion7()), user.AccessToken);

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, CityErrors.NotFound.Code);
        (await DbContext.Providers.AnyAsync(p => p.Name == name)).Should().BeFalse();
        (await GetRolesAsync(user.AccessToken)).Should().BeEquivalentTo([Role.Parent]);
    }

    [Fact]
    public async Task RegisterProvider_ShouldReturnNotFound_WhenDistrictDoesNotExist()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();

        // Act
        HttpResponseMessage response = await RegisterAsync(
            NewRequest(UniqueName(), district.CityId, Guid.CreateVersion7()), await ParentTokenAsync());

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, DistrictErrors.NotFound.Code);
    }

    [Fact]
    public async Task RegisterProvider_ShouldReturnBadRequest_WhenCategoryIsNotVenue()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await RegisterAsync(
            NewRequest(UniqueName(), cityId) with { Category = "Catering" }, await ParentTokenAsync());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RegisterProvider_ShouldReturnBadRequest_WhenLocationIsOutsideSerbia()
    {
        // Arrange: Budapest
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await RegisterAsync(
            NewRequest(UniqueName(), cityId) with { Latitude = 47.4979, Longitude = 19.0402 }, await ParentTokenAsync());

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.LocationOutsideSerbia.Code);
    }

    [Fact]
    public async Task RegisterProvider_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await RegisterAsync(NewRequest(UniqueName(), cityId), accessToken: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task RegisterProvider_ShouldReturnForbidden_WhenUserIsAdmin()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await RegisterAsync(NewRequest(UniqueName(), cityId), await AdminTokenAsync());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static RegisterProviderRequest NewRequest(string name, Guid cityId, Guid? districtId = null) =>
        new(name, "Venue", null, Faker.Address.StreetAddress(), cityId, districtId, Latitude, Longitude, null);

    private Task<HttpResponseMessage> RegisterAsync(RegisterProviderRequest request, string? accessToken) =>
        HttpClient.SendAsync(NewMessage(request, accessToken));

    private static HttpRequestMessage NewMessage(RegisterProviderRequest request, string? accessToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "providers") { Content = JsonContent.Create(request) };
        if (accessToken is not null)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return message;
    }

    private async Task<List<string>> GetRolesAsync(string accessToken)
    {
        HttpResponseMessage response = await HttpClient.GetCurrentUserAsync(accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<CurrentUserResponse>(JsonOptions))!.Roles;
    }

    private sealed record RegisterProviderRequest(
        string Name,
        string Category,
        string? Description,
        string Address,
        Guid CityId,
        Guid? DistrictId,
        double Latitude,
        double Longitude,
        string? Phone);

    private sealed class ThrowingProviderRepository : IProviderRepository
    {
        public Task<Provider?> GetByIdAsync(Guid providerId, CancellationToken cancellationToken) =>
            Task.FromResult<Provider?>(null);

        public Task<IReadOnlyList<string>> GetSlugsWithBaseAsync(string baseSlug, CancellationToken cancellationToken) =>
            Task.FromResult<IReadOnlyList<string>>([]);

        public uint GetVersion(Provider provider) => 0;

        public void Insert(Provider provider) => throw new InvalidOperationException("Simulated provider insert failure");
    }
}
