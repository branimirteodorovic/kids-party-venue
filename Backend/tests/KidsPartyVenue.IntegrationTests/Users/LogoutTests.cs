using System.Net;
using AwesomeAssertions;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.IntegrationTests.Users;

public sealed class LogoutTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Logout_ShouldRevokeFamily()
    {
        // Arrange: rotate once so the family holds a revoked and an active token
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);
        HttpResponseMessage rotateResponse = await HttpClient.RefreshAsync(user.RefreshToken);
        string activeToken = rotateResponse.GetRefreshTokenCookie()!;

        // Act
        HttpResponseMessage response = await HttpClient.LogoutAsync(activeToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);

        List<DateTime?> revokedAt = await DbContext.RefreshTokens
            .AsNoTracking()
            .Where(t => t.UserId == user.UserId)
            .Select(t => t.RevokedAtUtc)
            .ToListAsync();
        revokedAt.Should().HaveCount(2).And.OnlyContain(r => r != null);

        (await HttpClient.RefreshAsync(activeToken)).StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldClearCookie()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);

        // Act
        HttpResponseMessage response = await HttpClient.LogoutAsync(user.RefreshToken);

        // Assert
        string? setCookie = response.GetRefreshTokenSetCookie();
        setCookie.Should().NotBeNull();
        setCookie!.Should().ContainEquivalentOf("expires=Thu, 01 Jan 1970");
        response.GetRefreshTokenCookie().Should().BeNull();
    }

    [Fact]
    public async Task Logout_ShouldReturnNoContent_WhenThereIsNoCookie()
    {
        // Act
        HttpResponseMessage response = await HttpClient.PostAsync("auth/logout", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }
}
