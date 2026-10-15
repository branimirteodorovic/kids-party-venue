using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Users;

public sealed class RegisterTests(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    private readonly IntegrationTestWebAppFactory _factory = factory;

    [Fact]
    public async Task Register_ShouldReturnUserId_WhenRequestIsValid()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker);

        // Act
        HttpResponseMessage response = await HttpClient.RegisterAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        RegisterResponse? body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        body!.UserId.Should().NotBeEmpty();

        using IServiceScope scope = _factory.Services.CreateScope();
        UserManager<User> userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
        User? user = await userManager.FindByIdAsync(body.UserId.ToString());
        user.Should().NotBeNull();
        (await userManager.IsInRoleAsync(user!, Role.Parent)).Should().BeTrue();
    }

    [Fact]
    public async Task Register_ShouldReturnConflict_WhenEmailIsTaken()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker);
        await HttpClient.RegisterAsync(request);

        // Act
        HttpResponseMessage response = await HttpClient.RegisterAsync(request with { Email = request.Email.ToUpperInvariant() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_ShouldReturnValidationError_WhenPasswordTooShort()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker) with { Password = "short" };

        // Act
        HttpResponseMessage response = await HttpClient.RegisterAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
