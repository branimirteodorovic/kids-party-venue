using System.Net;
using AwesomeAssertions;
using KidsPartyVenue.Application.Providers.GetProviderForEditing;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class GetProviderForEditingTests(IntegrationTestWebAppFactory factory) : ProviderEditorTestBase(factory)
{
    [Fact]
    public async Task GetProviderForEditing_ShouldReturnDraft_WhenProviderExists()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        Provider saved = await DbContext.Providers.AsNoTracking().SingleAsync(p => p.Id == providerId);
        District district = await GetNoviSadDistrictAsync();

        // Act
        ProviderForEditingResponse provider = await GetAsAdminAsync(providerId);

        // Assert
        provider.Id.Should().Be(providerId);
        provider.Slug.Should().Be(saved.Slug);
        provider.Name.Should().Be(saved.Name);
        provider.Category.Should().Be(ProviderCategory.Venue);
        provider.Description.Should().Be("Opis");
        provider.Address.Should().Be(saved.Address);
        provider.CityId.Should().Be(district.CityId);
        provider.DistrictId.Should().Be(district.Id);
        provider.Latitude.Should().BeApproximately(Latitude, 1e-9);
        provider.Longitude.Should().BeApproximately(Longitude, 1e-9);
        provider.Phone.Should().Be("+381631234567");
        provider.Status.Should().Be(ProviderStatus.Draft);
        provider.IsClaimed.Should().BeFalse();
        provider.BookingPolicy.Should().BeNull();
        provider.VenueDetails.Should().BeNull();
        provider.AmenityIds.Should().BeEmpty();
        provider.Version.Should().NotBe(0u);
    }

    [Fact]
    public async Task GetProviderForEditing_ShouldReturnNotFound_WhenProviderDoesNotExist()
    {
        // Act
        HttpResponseMessage response = await GetRawAsAdminAsync(Guid.CreateVersion7());

        // Assert
        await ShouldFailWith(response, HttpStatusCode.NotFound, ProviderErrors.NotFound.Code);
    }

    [Fact]
    public async Task GetProviderForEditing_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();

        // Act
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, accessToken: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetProviderForEditing_ShouldReturnForbidden_WhenUserIsParent()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();

        // Act
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, await ParentTokenAsync());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
