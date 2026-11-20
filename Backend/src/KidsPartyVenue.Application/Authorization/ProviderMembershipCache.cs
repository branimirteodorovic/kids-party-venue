namespace KidsPartyVenue.Application.Authorization;

/// <summary>
/// A user's cached role permissions and provider memberships. A handler that changes either (a role, a membership)
/// calls <c>RemoveByTagAsync(UserTag(userId))</c> after saving, so the next request reads them again.
/// </summary>
public static class ProviderMembershipCache
{
    /// <summary>A fallback in case an invalidation is missed; writes remove the tag right away.</summary>
    public static readonly TimeSpan Expiration = TimeSpan.FromMinutes(30);

    public static string UserTag(Guid userId) => $"user:{userId}";

    public static string MembershipsKey(Guid userId) => $"provider-memberships:{userId}";

    public static string PermissionsKey(Guid userId) => $"permissions:{userId}";
}
