using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace KidsPartyVenue.Infrastructure.Identity;

internal sealed class PermissionService(UserManager<User> userManager, ILogger<PermissionService> logger) : IPermissionService
{
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId)
    {
        if (!Guid.TryParse(identityId, out Guid id))
        {
            logger.LogWarning("Failed to parse identityId: {IdentityId}", identityId);
            return Result.Failure<PermissionsResponse>(UserErrors.IdentityIdMalformed);
        }

        User? user = await userManager.FindByIdAsync(id.ToString());

        if (user is null)
        {
            return Result.Failure<PermissionsResponse>(UserErrors.NotFound);
        }

        IList<string> roles = await userManager.GetRolesAsync(user);
        HashSet<string> rolePermissions = [.. roles.SelectMany(RolePermissions.For)];

        return Result.Success(new PermissionsResponse(user.Id, rolePermissions));
    }

    public HashSet<string> GetPermissionsFromRoles(IEnumerable<string> roles)
    {
        return [.. roles.SelectMany(RolePermissions.For)];
    }
}