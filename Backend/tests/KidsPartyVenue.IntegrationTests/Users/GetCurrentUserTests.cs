using System.Net;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text;
using AwesomeAssertions;
using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Users.GetCurrentUser;
using KidsPartyVenue.Infrastructure.Authentication;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace KidsPartyVenue.IntegrationTests.Users;

public sealed class GetCurrentUserTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task GetCurrentUser_ShouldReturnPermissions_WhenTokenIsValid()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);

        // Act
        HttpResponseMessage response = await HttpClient.GetCurrentUserAsync(user.AccessToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        CurrentUserResponse? body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        body.Should().NotBeNull();
        body!.Id.Should().Be(user.UserId);
        body.Email.Should().Be(user.Request.Email);
        body.FullName.Should().Be(user.Request.FullName.Trim());
        body.Roles.Should().BeEquivalentTo([Role.Parent]);
        body.Permissions.Should().BeEquivalentTo([Permissions.UsersReadSelf, Permissions.ProvidersRegister]);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await HttpClient.GetCurrentUserAsync(accessToken: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenTokenIsExpired()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);
        string expiredToken = CreateToken(user, DateTime.UtcNow.AddHours(-1));

        // Act
        HttpResponseMessage response = await HttpClient.GetCurrentUserAsync(expiredToken);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnUnauthorized_WhenTokenIsSignedWithAnotherKey()
    {
        // Arrange: same claims and lifetime as a token the API accepts, so only the signature differs
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);
        string validToken = CreateToken(user, DateTime.UtcNow.AddMinutes(15));
        string forgedToken = CreateToken(user, DateTime.UtcNow.AddMinutes(15), signingKey: new string('x', 64));

        // Act
        HttpResponseMessage validResponse = await HttpClient.GetCurrentUserAsync(validToken);
        HttpResponseMessage forgedResponse = await HttpClient.GetCurrentUserAsync(forgedToken);

        // Assert
        validResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        forgedResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private string CreateToken(LoggedInUser user, DateTime expiresAtUtc, string? signingKey = null)
    {
        JwtOptions jwt = _factory.Services.GetRequiredService<IOptions<JwtOptions>>().Value;

        var descriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(
            [
                new Claim(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Request.Email)
            ]),
            NotBefore = expiresAtUtc.AddMinutes(-15),
            Expires = expiresAtUtc,
            Issuer = jwt.Issuer,
            Audience = jwt.Audience,
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signingKey ?? jwt.SigningKey)),
                SecurityAlgorithms.HmacSha256)
        };

        return new JsonWebTokenHandler().CreateToken(descriptor);
    }
}
