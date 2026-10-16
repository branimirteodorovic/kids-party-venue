using KidsPartyVenue.Domain;

namespace KidsPartyVenue.Application.Authorization;

public interface IPermissionService
{
    Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId);

    HashSet<string> GetPermissionsFromRoles(IEnumerable<string> roles);
}
