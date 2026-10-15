using Microsoft.AspNetCore.Http;

namespace KidsPartyVenue.Presentation.Users;

internal static class RefreshTokenCookieHelper
{
    public const string Name = "kpv_refresh";

    public static void Set(HttpResponse response, string token, DateTime expiresAtUtc) =>
        response.Cookies.Append(Name, token, new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Path = "/auth",
            Expires = expiresAtUtc
        });

    public static void Clear(HttpResponse response) =>
        response.Cookies.Delete(Name, new CookieOptions { Path = "/auth", Secure = true, SameSite = SameSiteMode.Strict });
}

