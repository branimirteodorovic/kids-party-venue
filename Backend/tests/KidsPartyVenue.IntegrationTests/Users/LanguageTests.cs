using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Application.Users.GetCurrentUser;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.IntegrationTests.Abstractions;

namespace KidsPartyVenue.IntegrationTests.Users;

public sealed class LanguageTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    [Theory]
    [InlineData("ru", Language.Ru)]
    [InlineData("en-US,en;q=0.9", Language.En)]
    // A lower-quality alternative makes a failed match visible, since sr is also the default
    [InlineData("sr-Latn-RS, en;q=0.5", Language.Sr)]
    [InlineData("sr-Cyrl, ru;q=0.5", Language.Sr)]
    [InlineData("hr, ru;q=0.5", Language.Ru)]
    [InlineData("de", Language.Sr)]
    [InlineData("*", Language.Sr)]
    [InlineData(null, Language.Sr)]
    public async Task Register_ShouldUseRequestLanguage_WhenPreferredLanguageIsOmitted(string? acceptLanguage, Language expected)
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker, acceptLanguage);

        // Act
        CurrentUserResponse body = await GetCurrentUserAsync(user.AccessToken);

        // Assert
        body.PreferredLanguage.Should().Be(expected);
    }

    [Fact]
    public async Task Register_ShouldUsePreferredLanguage_WhenItIsGiven()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker, acceptLanguage: "ru", preferredLanguage: "en");

        // Act
        CurrentUserResponse body = await GetCurrentUserAsync(user.AccessToken);

        // Assert
        body.PreferredLanguage.Should().Be(Language.En);
    }

    [Fact]
    public async Task Register_ShouldReturnBadRequest_WhenPreferredLanguageIsUnsupported()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker) with { PreferredLanguage = "de" };

        // Act
        HttpResponseMessage response = await HttpClient.RegisterAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ChangePreferredLanguage_ShouldUpdateLanguage_WhenLanguageIsSupported()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker, acceptLanguage: "sr");

        // Act
        HttpResponseMessage response = await HttpClient.ChangePreferredLanguageAsync(user.AccessToken, "ru");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
        (await GetCurrentUserAsync(user.AccessToken)).PreferredLanguage.Should().Be(Language.Ru);
    }

    [Fact]
    public async Task ChangePreferredLanguage_ShouldReturnBadRequest_WhenLanguageIsUnsupported()
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker, acceptLanguage: "en");

        // Act
        HttpResponseMessage response = await HttpClient.ChangePreferredLanguageAsync(user.AccessToken, "de");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        (await GetCurrentUserAsync(user.AccessToken)).PreferredLanguage.Should().Be(Language.En);
    }

    [Fact]
    public async Task ChangePreferredLanguage_ShouldReturnUnauthorized_WhenTokenIsMissing()
    {
        // Act
        HttpResponseMessage response = await HttpClient.ChangePreferredLanguageAsync(accessToken: null, "en");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Theory]
    [InlineData("ru", "ru")]
    [InlineData("en-GB", "en")]
    [InlineData(null, "sr")]
    public async Task Response_ShouldHaveContentLanguage_WhenRequestLanguageIsResolved(string? acceptLanguage, string expected)
    {
        // Arrange
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker);

        // Act
        HttpResponseMessage response = await HttpClient.GetCurrentUserAsync(user.AccessToken, acceptLanguage);

        // Assert
        response.Content.Headers.ContentLanguage.Should().Equal(expected);
    }

    [Fact]
    public async Task GetCurrentUser_ShouldReturnPreferredLanguage_WhenRequestLanguageDiffers()
    {
        // Arrange: the request language belongs to the page, the preferred language to the user
        LoggedInUser user = await HttpClient.RegisterAndLoginAsync(Faker, preferredLanguage: "ru");

        // Act
        HttpResponseMessage response = await HttpClient.GetCurrentUserAsync(user.AccessToken, acceptLanguage: "en");

        // Assert
        CurrentUserResponse? body = await response.Content.ReadFromJsonAsync<CurrentUserResponse>();
        body!.PreferredLanguage.Should().Be(Language.Ru);
    }

    private async Task<CurrentUserResponse> GetCurrentUserAsync(string accessToken)
    {
        HttpResponseMessage response = await HttpClient.GetCurrentUserAsync(accessToken);
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<CurrentUserResponse>())!;
    }
}
