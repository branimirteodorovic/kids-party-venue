using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.IntegrationTests.Users;

public sealed class RefreshTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Refresh_ShouldRotateToken_WhenTokenIsValid()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);

        // Act
        HttpResponseMessage response = await HttpClient.RefreshAsync(user.RefreshToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessToken.Should().NotBeNullOrEmpty();

        string? newRefreshToken = response.GetRefreshTokenCookie();
        newRefreshToken.Should().NotBeNullOrEmpty().And.NotBe(user.RefreshToken);

        RefreshToken old = await GetRefreshTokenAsync(user.RefreshToken);
        RefreshToken next = await GetRefreshTokenAsync(newRefreshToken!);

        old.RevokedAtUtc.Should().NotBeNull();
        old.ReplacedByTokenId.Should().Be(next.Id);
        next.RevokedAtUtc.Should().BeNull();
        next.FamilyId.Should().Be(old.FamilyId);
        next.UserId.Should().Be(user.UserId);

        (await HttpClient.GetCurrentUserAsync(body.AccessToken)).StatusCode.Should().Be(HttpStatusCode.OK);
        (await HttpClient.RefreshAsync(newRefreshToken!)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_ShouldStoreOnlyTheRefreshTokenHash()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);

        // Act
        bool storedRaw = await DbContext.RefreshTokens.AsNoTracking().AnyAsync(t => t.TokenHash == user.RefreshToken);
        bool storedHash = await DbContext.RefreshTokens.AsNoTracking()
            .AnyAsync(t => t.TokenHash == UserRequests.HashRefreshToken(user.RefreshToken));

        // Assert
        storedRaw.Should().BeFalse();
        storedHash.Should().BeTrue();
    }

    [Fact]
    public async Task Refresh_ShouldRevokeFamily_WhenRevokedTokenIsReused()
    {
        // Arrange: A -> B
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);
        string tokenA = user.RefreshToken;
        HttpResponseMessage rotateResponse = await HttpClient.RefreshAsync(tokenA);
        string tokenB = rotateResponse.GetRefreshTokenCookie()!;

        // Act: A is already revoked, so presenting it again looks like theft
        HttpResponseMessage reuseResponse = await HttpClient.RefreshAsync(tokenA);
        HttpResponseMessage childResponse = await HttpClient.RefreshAsync(tokenB);

        // Assert
        reuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        childResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        (await GetRefreshTokenAsync(tokenA)).RevokedAtUtc.Should().NotBeNull();
        (await GetRefreshTokenAsync(tokenB)).RevokedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task Refresh_ShouldNotAffectOtherSessions_WhenRevokedTokenIsReused()
    {
        // Arrange: the same user logged in twice gives two families
        LoggedInUser first = await HttpClient.RegisterAndLoginAsync(Faker);
        HttpResponseMessage secondLogin = await HttpClient.LoginAsync(first.Request.Email, first.Request.Password);
        string otherSessionToken = secondLogin.GetRefreshTokenCookie()!;

        await HttpClient.RefreshAsync(first.RefreshToken);

        // Act
        await HttpClient.RefreshAsync(first.RefreshToken);

        // Assert
        (await GetRefreshTokenAsync(otherSessionToken)).RevokedAtUtc.Should().BeNull();
        (await HttpClient.RefreshAsync(otherSessionToken)).StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenTokenIsUnknown()
    {
        // Act
        HttpResponseMessage response = await HttpClient.RefreshAsync("not-a-real-token");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_ShouldReturnUnauthorized_WhenTokenIsExpired()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);
        string hash = UserRequests.HashRefreshToken(user.RefreshToken);
        DateTime expiredAtUtc = DateTime.UtcNow.AddMinutes(-1);
        await DbContext.RefreshTokens
            .Where(t => t.TokenHash == hash)
            .ExecuteUpdateAsync(s => s.SetProperty(t => t.ExpiresAtUtc, expiredAtUtc));

        // Act
        HttpResponseMessage response = await HttpClient.RefreshAsync(user.RefreshToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private Task<RefreshToken> GetRefreshTokenAsync(string rawToken)
    {
        string hash = UserRequests.HashRefreshToken(rawToken);

        return DbContext.RefreshTokens.AsNoTracking().SingleAsync(t => t.TokenHash == hash);
    }
}
