using System.Net;
using AwesomeAssertions;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Providers.GetProviderForEditing;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using KidsPartyVenue.IntegrationTests.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Providers;

public sealed class ProviderMembershipAuthorizationTests(IntegrationTestWebAppFactory factory) : ProviderEditorTestBase(factory)
{
    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task GetProviderForEditing_ShouldReturnProvider_WhenUserIsOwner()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        LoggedInUser owner = await HttpClient.RegisterAndLoginAsync(Faker);
        await AddOwnerAsync(providerId, owner.UserId);

        // Act
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, owner.AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Update_ShouldSucceed_WhenUserIsOwner()
    {
        // Arrange
        Guid providerId = await CreateProviderAsync();
        LoggedInUser owner = await HttpClient.RegisterAndLoginAsync(Faker);
        await AddOwnerAsync(providerId, owner.UserId);
        ProviderForEditingResponse current = await GetAsAdminAsync(providerId);
        List<Guid> amenityIds = await GetAmenityIdsAsync(2);

        // Act
        HttpResponseMessage response = await SendAsync(
            HttpMethod.Put, $"providers/{providerId}/amenities", new { amenityIds, current.Version }, owner.AccessToken);

        // Assert
        await ReadVersionAsync(response);
        (await GetAsAdminAsync(providerId)).AmenityIds.Should().BeEquivalentTo(amenityIds);
    }

    [Fact]
    public async Task GetProviderForEditing_ShouldReturnForbidden_WhenUserOwnsAnotherProvider()
    {
        // Arrange
        Guid ownProviderId = await CreateProviderAsync();
        Guid otherProviderId = await CreateProviderAsync();
        LoggedInUser owner = await HttpClient.RegisterAndLoginAsync(Faker);
        await AddOwnerAsync(ownProviderId, owner.UserId);

        // Act
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"providers/{otherProviderId}", null, owner.AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProviderForEditing_ShouldReturnForbidden_WhenProviderDoesNotExistAndUserIsNotAdmin()
    {
        // Act
        HttpResponseMessage response = await SendAsync(
            HttpMethod.Get, $"providers/{Guid.CreateVersion7()}", null, await ParentTokenAsync());

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetProviderForEditing_ShouldReturnForbidden_WhenOwnerWasRemovedAndUserTagWasInvalidated()
    {
        // Arrange: two owners, so one of them can be removed
        Guid providerId = await CreateProviderAsync();
        LoggedInUser leaving = await HttpClient.RegisterAndLoginAsync(Faker);
        LoggedInUser staying = await HttpClient.RegisterAndLoginAsync(Faker);
        await AddOwnerAsync(providerId, leaving.UserId);
        await AddOwnerAsync(providerId, staying.UserId);
        (await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, leaving.AccessToken))
            .StatusCode.Should().Be(HttpStatusCode.OK);

        await RemoveOwnerAsync(providerId, leaving.UserId);
        HttpResponseMessage beforeInvalidation = await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, leaving.AccessToken);

        // Act
        await RemoveUserTagAsync(leaving.UserId);
        HttpResponseMessage afterInvalidation = await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, leaving.AccessToken);

        // Assert: the cached membership is used until the tag is removed
        beforeInvalidation.StatusCode.Should().Be(HttpStatusCode.OK);
        afterInvalidation.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserPermissions_ShouldReadRolesAgain_WhenUserTagWasInvalidated()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);
        HashSet<string> before = await GetPermissionsAsync(user.UserId);

        using (IServiceScope scope = _factory.Services.CreateScope())
        {
            UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            User identityUser = (await userManager.FindByIdAsync(user.UserId.ToString()))!;
            (await userManager.AddToRoleAsync(identityUser, Role.Admin)).Succeeded.Should().BeTrue();
        }

        HashSet<string> cached = await GetPermissionsAsync(user.UserId);

        // Act
        await RemoveUserTagAsync(user.UserId);
        HashSet<string> after = await GetPermissionsAsync(user.UserId);

        // Assert
        before.Should().NotContain(Permissions.ProvidersManageAny);
        cached.Should().BeEquivalentTo(before);
        after.Should().Contain(Permissions.ProvidersManageAny);
    }

    private async Task AddOwnerAsync(Guid providerId, Guid userId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        Provider provider = (await scope.ServiceProvider.GetRequiredService<IProviderRepository>()
            .GetByIdAsync(providerId, CancellationToken.None))!;
        provider.AddMember(userId, ProviderMemberRole.Owner, DateTime.UtcNow).IsSuccess.Should().BeTrue();
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
    }

    private async Task RemoveOwnerAsync(Guid providerId, Guid userId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        Provider provider = (await scope.ServiceProvider.GetRequiredService<IProviderRepository>()
            .GetByIdAsync(providerId, CancellationToken.None))!;
        provider.RemoveMember(userId, DateTime.UtcNow).IsSuccess.Should().BeTrue();
        await scope.ServiceProvider.GetRequiredService<IUnitOfWork>().SaveChangesAsync(CancellationToken.None);
    }

    private async Task RemoveUserTagAsync(Guid userId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        await scope.ServiceProvider.GetRequiredService<ICacheService>().RemoveByTagAsync(ProviderMembershipCache.UserTag(userId));
    }

    private async Task<HashSet<string>> GetPermissionsAsync(Guid userId)
    {
        using IServiceScope scope = _factory.Services.CreateScope();
        Result<PermissionsResponse> result = await scope.ServiceProvider.GetRequiredService<IPermissionService>()
            .GetUserPermissionsAsync(userId.ToString(), CancellationToken.None);
        result.IsSuccess.Should().BeTrue();

        return result.Value.Permissions;
    }
}
