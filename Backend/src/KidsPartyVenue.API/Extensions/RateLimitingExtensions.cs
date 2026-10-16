using System.Threading.RateLimiting;
using KidsPartyVenue.Presentation.RateLimiting;
using Microsoft.Extensions.Options;

namespace KidsPartyVenue.API.Extensions;

internal static class RateLimitingExtensions
{
    public static IServiceCollection AddRateLimiting(this IServiceCollection services)
    {
        services.AddOptions<AuthRateLimitOptions>()
            .BindConfiguration(AuthRateLimitOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            // Options are resolved per request so tests can override them after the host is built.
            options.AddPolicy(RateLimitPolicies.Auth, httpContext =>
            {
                AuthRateLimitOptions limits = httpContext.RequestServices
                    .GetRequiredService<IOptions<AuthRateLimitOptions>>().Value;

                return RateLimitPartition.GetFixedWindowLimiter(
                    httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                    _ => new FixedWindowRateLimiterOptions
                    {
                        PermitLimit = limits.PermitLimit,
                        Window = TimeSpan.FromSeconds(limits.WindowSeconds),
                        QueueLimit = 0
                    });
            });
        });

        return services;
    }
}
