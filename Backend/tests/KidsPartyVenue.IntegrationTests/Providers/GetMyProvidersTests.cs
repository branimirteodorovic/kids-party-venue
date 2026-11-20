using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Application.Providers.GetMyProviders;
using KidsPartyVenue.Application.Providers.RegisterProvider;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.IntegrationTests.Abstractions;
using KidsPartyVenue.IntegrationTests.Users;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class GetMyProvidersTests(IntegrationTestWebAppFactory factory) : ProviderEditorTestBase(factory)
{
    [Fact]
    public async Task GetMyProviders_ShouldReturnOwnProvidersInOrder_WhenUserOwnsTwo()
    {
        // Arrange
        Guid cityId = await GetCityIdAsync(CityWithoutDistrictsSlug);
        LoggedInUser owner = await HttpClient.RegisterAndLoginAsync(Faker);
        LoggedInUser otherOwner = await HttpClient.RegisterAndLoginAsync(Faker);
        string firstName = UniqueName();
        string secondName = UniqueName();
        RegisterProviderResponse first = await RegisterAsync(firstName, cityId, owner.AccessToken);
        RegisterProviderResponse second = await RegisterAsync(secondName, cityId, owner.AccessToken);
        await RegisterAsync(UniqueName(), cityId, otherOwner.AccessToken);

        // Act
        List<MyProviderResponse> providers = await GetMyProvidersAsync(owner.AccessToken);

        // Assert
        providers.Should().BeEquivalentTo(
            [
                new MyProviderResponse { Id = first.Id, Slug = first.Slug, Name = firstName, Status = ProviderStatus.Draft, Role = ProviderMemberRole.Owner },
                new MyProviderResponse { Id = second.Id, Slug = second.Slug, Name = secondName, Status = ProviderStatus.Draft, Role = ProviderMemberRole.Owner }
            ],
            options => options.WithStrictOrdering());
    }

    [Fact]
    public async Task GetMyProviders_ShouldReturnEmptyList_WhenUserIsParent()
    {
        // Act
        List<MyProviderResponse> providers = await GetMyProvidersAsync(await ParentTokenAsync());

        // Assert
        providers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyProviders_ShouldReturnEmptyList_WhenUserIsAdmin()
    {
        // Arrange: the admin reaches every provider through a permission, not through memberships
        await CreateProviderAsync();

        // Act
        List<MyProviderResponse> providers = await GetMyProvidersAsync(await AdminTokenAsync());

        // Assert
        providers.Should().BeEmpty();
    }

    [Fact]
    public async Task GetMyProviders_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, "users/me/providers", null, accessToken: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private async Task<RegisterProviderResponse> RegisterAsync(string name, Guid cityId, string accessToken)
    {
        object request = new
        {
            name,
            category = "Venue",
            address = Faker.Address.StreetAddress(),
            cityId,
            latitude = Latitude,
            longitude = Longitude
        };
        HttpResponseMessage response = await SendAsync(HttpMethod.Post, "providers", request, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        return (await response.Content.ReadFromJsonAsync<RegisterProviderResponse>(JsonOptions))!;
    }

    private async Task<List<MyProviderResponse>> GetMyProvidersAsync(string accessToken)
    {
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, "users/me/providers", null, accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<List<MyProviderResponse>>(JsonOptions))!;
    }
}
