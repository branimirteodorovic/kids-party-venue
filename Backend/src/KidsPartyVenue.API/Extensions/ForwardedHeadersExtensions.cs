using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

namespace KidsPartyVenue.API.Extensions;

internal static class ForwardedHeadersExtensions
{
    /// <summary>
    /// Makes <c>RemoteIpAddress</c> the real client IP behind a reverse proxy. Only proxies listed in
    /// <c>ForwardedHeaders:KnownProxies</c> are trusted (plus loopback by default), so clients cannot spoof the header.
    /// </summary>
    public static IServiceCollection AddForwardedHeaders(this IServiceCollection services, IConfiguration configuration)
    {
        string[] knownProxies = configuration.GetSection("ForwardedHeaders:KnownProxies").Get<string[]>() ?? [];

        services.Configure<ForwardedHeadersOptions>(options =>
        {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

            foreach (string proxy in knownProxies)
            {
                options.KnownProxies.Add(IPAddress.Parse(proxy));
            }
        });

        return services;
    }
}
