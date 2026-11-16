using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Application.Providers.CreateProvider;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using KidsPartyVenue.IntegrationTests.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class CreateProviderTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    // Beograd is hidden and has no districts in the seed
    private const string CityWithoutDistrictsSlug = "beograd";

    // Inside Serbia, near the center of Novi Sad
    private const double Latitude = 45.2389;
    private const double Longitude = 19.8441;

    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task CreateProvider_ShouldCreateUnclaimedDraft_WhenRequestIsValid()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();
        string name = UniqueName();
        CreateProviderRequest request = NewRequest(name, district.CityId, district.Id) with { Phone = "063 123 4567", Description = "   " };
        DateTime before = DateTime.UtcNow;

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().BeNull();
        CreateProviderResponse body = (await response.Content.ReadFromJsonAsync<CreateProviderResponse>())!;

        using IServiceScope scope = _factory.Services.CreateScope();
        Provider saved = (await scope.ServiceProvider.GetRequiredService<IProviderRepository>()
            .GetByIdAsync(body.Id, CancellationToken.None))!;
        saved.Slug.Should().Be(body.Slug);
        saved.Name.Should().Be(name);
        saved.Category.Should().Be(ProviderCategory.Venue);
        saved.Description.Should().BeNull();
        saved.CityId.Should().Be(district.CityId);
        saved.DistrictId.Should().Be(district.Id);
        saved.Location.Latitude.Should().Be(Latitude);
        saved.Location.Longitude.Should().Be(Longitude);
        saved.Phone!.Value.Should().Be("+381631234567");
        saved.Status.Should().Be(ProviderStatus.Draft);
        saved.IsClaimed.Should().BeFalse();
        saved.BookingPolicy.Should().BeNull();
        saved.VenueDetails.Should().BeNull();
        saved.Amenities.Should().BeEmpty();
        saved.CreatedAtUtc.Should().BeOnOrAfter(before.AddSeconds(-1)).And.Be(saved.UpdatedAtUtc);
    }

    [Fact]
    public async Task CreateProvider_ShouldAddSuffixToSlug_WhenNameIsTaken()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);
        string name = UniqueName();
        HttpResponseMessage first = await PostAsAdminAsync(NewRequest(name, cityId));
        CreateProviderResponse firstBody = (await first.Content.ReadFromJsonAsync<CreateProviderResponse>())!;

        // Act
        HttpResponseMessage second = await PostAsAdminAsync(NewRequest(name, cityId));

        // Assert
        second.StatusCode.Should().Be(HttpStatusCode.Created);
        CreateProviderResponse secondBody = (await second.Content.ReadFromJsonAsync<CreateProviderResponse>())!;
        secondBody.Slug.Should().Be($"{firstBody.Slug}-2");
    }

    [Fact]
    public async Task CreateProvider_ShouldCreateWithoutDistrict_WhenCityIsHiddenAndHasNoDistricts()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(NewRequest(UniqueName(), cityId));

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        CreateProviderResponse body = (await response.Content.ReadFromJsonAsync<CreateProviderResponse>())!;
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == body.Id);
        saved.DistrictId.Should().BeNull();
        saved.Phone.Should().BeNull();
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnBadRequest_WhenDistrictIsMissingInCityWithDistricts()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(NewRequest(UniqueName(), district.CityId));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.DistrictRequired.Code);
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnBadRequest_WhenDistrictIsInAnotherCity()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();
        Guid otherCityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(NewRequest(UniqueName(), otherCityId, district.Id));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.DistrictNotInCity.Code);
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnNotFound_WhenCityDoesNotExist()
    {
        // Arrange
        string name = UniqueName();

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(NewRequest(name, Guid.CreateVersion7()));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, CityErrors.NotFound.Code);
        (await DbContext.Providers.AnyAsync(p => p.Name == name)).Should().BeFalse();
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnNotFound_WhenDistrictDoesNotExist()
    {
        // Arrange
        District district = await GetNoviSadDistrictAsync();
        string name = UniqueName();

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(NewRequest(name, district.CityId, Guid.CreateVersion7()));

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, DistrictErrors.NotFound.Code);
        (await DbContext.Providers.AnyAsync(p => p.Name == name)).Should().BeFalse();
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnBadRequest_WhenCategoryIsNotVenue()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(NewRequest(UniqueName(), cityId) with { Category = "Catering" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnBadRequest_WhenLocationIsOutsideSerbia()
    {
        // Arrange: Budapest
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(NewRequest(UniqueName(), cityId) with { Latitude = 47.4979, Longitude = 19.0402 });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, ProviderErrors.LocationOutsideSerbia.Code);
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnBadRequest_WhenPhoneIsInvalid()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await PostAsAdminAsync(NewRequest(UniqueName(), cityId) with { Phone = "12" });

        // Assert
        await ShouldFailWith(response, HttpStatusCode.BadRequest, "Phones.InvalidNumber");
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);

        // Act
        HttpResponseMessage response = await PostAsync(NewRequest(UniqueName(), cityId), accessToken: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateProvider_ShouldReturnForbidden_WhenUserIsParent()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);
        LoggedInUser parent = await HttpClient.RegisterAndLoginAsync(Faker);

        // Act
        HttpResponseMessage response = await PostAsync(NewRequest(UniqueName(), cityId), parent.AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private static CreateProviderRequest NewRequest(string name, Guid cityId, Guid? districtId = null) =>
        new(name, "Venue", null, Faker.Address.StreetAddress(), cityId, districtId, Latitude, Longitude, null);

    private static string UniqueName() => $"{Faker.Company.CompanyName()} {Guid.CreateVersion7():N}";

    private async Task<HttpResponseMessage> PostAsAdminAsync(CreateProviderRequest request) =>
        await PostAsync(request, await LoginAsAdminAsync());

    private Task<HttpResponseMessage> PostAsync(CreateProviderRequest request, string? accessToken)
    {
        var message = new HttpRequestMessage(HttpMethod.Post, "admin/providers") { Content = JsonContent.Create(request) };
        if (accessToken is not null)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return HttpClient.SendAsync(message);
    }

    private async Task<string> LoginAsAdminAsync()
    {
        IConfiguration configuration = _factory.Services.GetRequiredService<IConfiguration>();
        HttpResponseMessage response = await HttpClient.LoginAsync(configuration["Seed:AdminEmail"]!, configuration["Seed:AdminPassword"]!);
        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;
    }

    private async Task<District> GetNoviSadDistrictAsync()
    {
        Guid noviSadId = await GetCityIdAsync(DevelopmentSeeder.NoviSadSlug);

        return await DbContext.Districts.AsNoTracking().FirstAsync(d => d.CityId == noviSadId);
    }

    private Task<Guid> GetCityIdAsync(string slug) =>
        DbContext.Cities.AsNoTracking().Where(c => c.Slug == slug).Select(c => c.Id).SingleAsync();

    private static async Task ShouldFailWith(HttpResponseMessage response, HttpStatusCode statusCode, string errorCode)
    {
        response.StatusCode.Should().Be(statusCode);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be(errorCode);
    }

    private sealed record CreateProviderRequest(
        string Name,
        string Category,
        string? Description,
        string Address,
        Guid CityId,
        Guid? DistrictId,
        double Latitude,
        double Longitude,
        string? Phone);
}
