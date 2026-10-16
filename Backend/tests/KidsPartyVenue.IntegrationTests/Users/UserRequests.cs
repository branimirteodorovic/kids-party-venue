using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using Bogus;

namespace KidsPartyVenue.IntegrationTests.Users;

internal sealed record RegisterRequest(string Email, string Password, string FullName);

internal sealed record RegisterResponse(Guid UserId);

internal sealed record LoginRequest(string Email, string Password);

internal sealed record LoginResponse(string AccessToken, DateTime AccessTokenExpiresAtUtc);

internal sealed record LoggedInUser(Guid UserId, RegisterRequest Request, string AccessToken, string RefreshToken);

internal static class UserRequests
{
    private const string RefreshCookieName = "kpv_refresh";

    // Identity's defaults require a digit and a lowercase letter and limit user name characters,
    // so build values that always satisfy them instead of relying on random Bogus output.
    public static RegisterRequest NewRegisterRequest(Faker faker) =>
        new(NewEmail(), $"{faker.Internet.Password(10)}a1", faker.Name.FullName());

    public static string NewEmail() => $"user-{Guid.CreateVersion7():N}@example.test";

    public static Task<HttpResponseMessage> RegisterAsync(this HttpClient client, RegisterRequest request) =>
        client.PostAsJsonAsync("auth/register", request);

    public static Task<HttpResponseMessage> LoginAsync(this HttpClient client, string email, string password) =>
        client.PostAsJsonAsync("auth/login", new LoginRequest(email, password));

    public static Task<HttpResponseMessage> RefreshAsync(this HttpClient client, string refreshToken) =>
        client.PostWithRefreshCookieAsync("auth/refresh", refreshToken);

    public static Task<HttpResponseMessage> LogoutAsync(this HttpClient client, string refreshToken) =>
        client.PostWithRefreshCookieAsync("auth/logout", refreshToken);

    public static Task<HttpResponseMessage> GetCurrentUserAsync(this HttpClient client, string? accessToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "users/me");
        if (accessToken is not null)
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return client.SendAsync(request);
    }

    public static async Task<LoggedInUser> RegisterAndLoginAsync(this HttpClient client, Faker faker)
    {
        RegisterRequest request = NewRegisterRequest(faker);

        HttpResponseMessage registerResponse = await client.RegisterAsync(request);
        RegisterResponse registered = (await registerResponse.Content.ReadFromJsonAsync<RegisterResponse>())!;

        HttpResponseMessage loginResponse = await client.LoginAsync(request.Email, request.Password);
        LoginResponse login = (await loginResponse.Content.ReadFromJsonAsync<LoginResponse>())!;

        return new LoggedInUser(registered.UserId, request, login.AccessToken, loginResponse.GetRefreshTokenCookie()!);
    }

    /// <summary>Returns the raw value of the refresh cookie set by the response, or null if it sets none.</summary>
    public static string? GetRefreshTokenCookie(this HttpResponseMessage response)
    {
        string? setCookie = response.GetRefreshTokenSetCookie();
        if (setCookie is null)
        {
            return null;
        }

        string value = setCookie[(RefreshCookieName.Length + 1)..].Split(';')[0];

        return value.Length == 0 ? null : value;
    }

    public static string? GetRefreshTokenSetCookie(this HttpResponseMessage response) =>
        response.Headers.TryGetValues("Set-Cookie", out IEnumerable<string>? values)
            ? values.FirstOrDefault(v => v.StartsWith($"{RefreshCookieName}=", StringComparison.Ordinal))
            : null;

    // Same algorithm as IdentityService: only this hash is stored in identity.refresh_tokens
    public static string HashRefreshToken(string token) =>
        Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token)));

    private static Task<HttpResponseMessage> PostWithRefreshCookieAsync(this HttpClient client, string path, string refreshToken)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, path);
        request.Headers.Add("Cookie", $"{RefreshCookieName}={refreshToken}");

        return client.SendAsync(request);
    }
}
