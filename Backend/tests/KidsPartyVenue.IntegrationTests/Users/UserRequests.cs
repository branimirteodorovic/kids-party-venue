using System.Net.Http.Json;
using Bogus;

namespace KidsPartyVenue.IntegrationTests.Users;

internal sealed record RegisterRequest(string Email, string Password, string FullName);

internal sealed record RegisterResponse(Guid UserId);

internal sealed record LoginRequest(string Email, string Password);

internal sealed record LoginResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc);

internal static class UserRequests
{
    // Identity's defaults require a digit and a lowercase letter and limit user name characters,
    // so build values that always satisfy them instead of relying on random Bogus output.
    public static RegisterRequest NewRegisterRequest(Faker faker) =>
        new(NewEmail(), $"{faker.Internet.Password(10)}a1", faker.Name.FullName());

    public static string NewEmail() => $"user-{Guid.CreateVersion7():N}@example.test";

    public static Task<HttpResponseMessage> RegisterAsync(this HttpClient client, RegisterRequest request) =>
        client.PostAsJsonAsync("auth/register", request);

    public static Task<HttpResponseMessage> LoginAsync(this HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("auth/login", new LoginRequest(email, password));
}
