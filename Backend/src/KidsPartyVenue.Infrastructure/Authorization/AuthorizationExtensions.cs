using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Infrastructure.Identity;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.Infrastructure.Authorization;

internal static class AuthorizationExtensions
{
    internal static IServiceCollection AddAuthorizationInternal(this IServiceCollection services)
    {
        services.AddTransient<IClaimsTransformation, CustomClaimsTransformation>();

        services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddTransient<IAuthorizationHandler, ProviderMemberAuthorizationHandler>();

        services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

        services.AddAuthorizationBuilder()
            .AddPolicy(AuthorizationPolicies.ProviderMember, policy => policy.AddRequirements(new ProviderMemberRequirement()));

        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }
}
