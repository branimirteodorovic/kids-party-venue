using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.IdentityModel.JsonWebTokens;

namespace KidsPartyVenue.IntegrationTests.Users;

public sealed class LoginTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Fact]
    public async Task Login_ShouldReturnAccessTokenAndSetCookie_WhenCredentialsAreValid()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker);
        HttpResponseMessage registerResponse = await HttpClient.RegisterAsync(request);
        RegisterResponse? registered = await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>();

        // Act
        HttpResponseMessage response = await HttpClient.LoginAsync(request.Email, request.Password);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        LoginResponse? body = await response.Content.ReadFromJsonAsync<LoginResponse>();
        body!.AccessTokenExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);

        var token = new JsonWebToken(body.AccessToken);
        token.Subject.Should().Be(registered!.UserId.ToString());
        token.GetClaim(JwtRegisteredClaimNames.Email).Value.Should().Be(request.Email);

        string? setCookie = response.GetRefreshTokenSetCookie();
        setCookie.Should().NotBeNull();
        setCookie!.Should().ContainEquivalentOf("httponly")
            .And.ContainEquivalentOf("secure")
            .And.ContainEquivalentOf("samesite=strict")
            .And.ContainEquivalentOf("path=/api/auth");
        response.GetRefreshTokenCookie().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenPasswordIsWrong()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker);
        await HttpClient.RegisterAsync(request);

        // Act
        HttpResponseMessage response = await HttpClient.LoginAsync(request.Email, request.Password + "x");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnUnauthorized_WhenUserDoesNotExist()
    {
        // Act
        HttpResponseMessage response = await HttpClient.LoginAsync(UserRequests.NewEmail(), "password1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_ShouldReturnLockedOut_AfterFiveFailedAttempts()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker);
        await HttpClient.RegisterAsync(request);

        for (int i = 0; i < 5; i++)
        {
            await HttpClient.LoginAsync(request.Email, request.Password + "x");
        }

        // Act: even the correct password is rejected while locked out
        HttpResponseMessage response = await HttpClient.LoginAsync(request.Email, request.Password);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }
}
