using KidsPartyVenue.Application.Authorization;

namespace KidsPartyVenue.Infrastructure.Identity;

internal static class RolePermissions
{
    private static readonly Dictionary<string, string[]> Map = new()
    {
        [Role.Parent] = [Permissions.UsersReadSelf, Permissions.ProvidersRegister],
        [Role.ProviderMember] = [Permissions.UsersReadSelf, Permissions.ProvidersRegister],
        [Role.Admin] = [Permissions.UsersReadSelf, Permissions.ProvidersCreate, Permissions.ProvidersReview,
                        Permissions.ProvidersManageAny, Permissions.BillingManage, Permissions.AuditRead]
    };

    public static IEnumerable<string> For(string role) => Map.TryGetValue(role, out string[]? p) ? p : [];
}

