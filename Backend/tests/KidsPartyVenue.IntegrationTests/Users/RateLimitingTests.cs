using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.API.Extensions;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Users;

public sealed class RateLimitingTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Theory]
    [InlineData("auth/register")]
    [InlineData("auth/login")]
    [InlineData("auth/refresh")]
    public async Task Auth_ShouldReturn429_WhenRateLimitExceeded(string path)
    {
        // Arrange
        const int permitLimit = 3;

        WebApplicationFactory<Program> limitedFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.PostConfigure<AuthRateLimitOptions>(options => options.PermitLimit = permitLimit)));
        using HttpClient client = limitedFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost/api/")
        });

        // Act
        List<HttpStatusCode> statusCodes = [];
        for (int i = 0; i < permitLimit + 1; i++)
        {
            HttpResponseMessage response = await client.PostAsJsonAsync(path, NewBody(path));
            statusCodes.Add(response.StatusCode);
        }

        // Assert
        statusCodes.Take(permitLimit).Should().NotContain(HttpStatusCode.TooManyRequests);
        statusCodes.Last().Should().Be(HttpStatusCode.TooManyRequests);
    }

    private static object NewBody(string path) => path switch
    {
        "auth/register" => UserRequests.NewRegisterRequest(Faker),
        "auth/login" => new LoginRequest(UserRequests.NewEmail(), "password1"),
        _ => new { }
    };
}
