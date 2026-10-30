using System.Net;
using System.Net.Http.Json;
using AwesomeAssertions;
using KidsPartyVenue.Domain.Users;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
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
    public async Task Register_ShouldCreateProfile_WhenRequestIsValid()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker) with { FullName = $"  {Faker.Name.FullName()} " };

        // Act
        HttpResponseMessage response = await HttpClient.RegisterAsync(request);

        // Assert
        RegisterResponse? body = await response.Content.ReadFromJsonAsync<RegisterResponse>();
        UserProfile? profile = await DbContext.UserProfiles.AsNoTracking().SingleOrDefaultAsync(p => p.Id == body!.UserId);
        profile.Should().NotBeNull();
        profile!.FullName.Should().Be(request.FullName.Trim());
    }

    [Fact]
    public async Task Register_ShouldCreateNoRecords_WhenEmailIsTaken()
    {
        // Arrange
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker);
        await HttpClient.RegisterAsync(request);
        int usersBefore = await DbContext.Users.CountAsync();
        int profilesBefore = await DbContext.UserProfiles.CountAsync();

        // Act
        HttpResponseMessage response = await HttpClient.RegisterAsync(request with { FullName = Faker.Name.FullName() });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
        (await DbContext.Users.CountAsync()).Should().Be(usersBefore);
        (await DbContext.UserProfiles.CountAsync()).Should().Be(profilesBefore);
    }

    [Fact]
    public async Task Register_ShouldCreateNoRecords_WhenProfileInsertFails()
    {
        // Arrange: the Identity user and its role are saved before the profile, inside the same transaction
        WebApplicationFactory<Program> failingFactory = _factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
                services.AddScoped<IUserProfileRepository, ThrowingUserProfileRepository>()));
        using HttpClient client = failingFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("http://localhost/api/")
        });
        RegisterRequest request = UserRequests.NewRegisterRequest(Faker);

        // Act
        HttpResponseMessage response = await client.RegisterAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
        (await DbContext.Users.AnyAsync(u => u.Email == request.Email)).Should().BeFalse();
    }

    [Fact]
    public async Task IdentityUsers_ShouldNotHaveFullName()
    {
        // Act
        int columns = await DbContext.Database
            .SqlQuery<int>(
                $"""
                 SELECT count(*)::int AS "Value"
                 FROM information_schema.columns
                 WHERE table_schema = 'identity' AND table_name = 'users' AND column_name = 'full_name'
                 """)
            .SingleAsync();

        // Assert
        columns.Should().Be(0);
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

    private sealed class ThrowingUserProfileRepository : IUserProfileRepository
    {
        public Task<UserProfile?> GetByIdAsync(Guid userId, CancellationToken cancellationToken) =>
            Task.FromResult<UserProfile?>(null);

        public void Insert(UserProfile profile) => throw new InvalidOperationException("Simulated profile insert failure");
    }
}
