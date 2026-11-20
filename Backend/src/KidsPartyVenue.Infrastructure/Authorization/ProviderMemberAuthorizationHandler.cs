using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace KidsPartyVenue.Infrastructure.Authorization;

/// <summary>
/// Reads the provider ID from the route and checks the user's cached memberships. An unknown provider fails like any
/// provider the user isn't a member of, so IDs can't be probed.
/// </summary>
internal sealed class ProviderMemberAuthorizationHandler(IPermissionService permissionService)
    : AuthorizationHandler<ProviderMemberRequirement>
{
    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        ProviderMemberRequirement requirement)
    {
        if (context.User.Identity?.IsAuthenticated != true)
        {
            return;
        }

        if (context.User.GetPermissions().Contains(Permissions.ProvidersManageAny))
        {
            context.Succeed(requirement);
            return;
        }

        if (context.Resource is not HttpContext httpContext
            || !Guid.TryParse(httpContext.GetRouteValue(AuthorizationPolicies.ProviderIdRouteValue)?.ToString(), out Guid providerId))
        {
            return;
        }

        IReadOnlyDictionary<Guid, ProviderMemberRole> memberships = await permissionService.GetProviderMembershipsAsync(
            context.User.GetUserId(),
            httpContext.RequestAborted);

        if (memberships.ContainsKey(providerId))
        {
            context.Succeed(requirement);
        }
    }
}
