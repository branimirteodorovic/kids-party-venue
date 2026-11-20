using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Authorization;

public interface IPermissionService
{
    /// <summary>The permissions of the user's roles, cached per user (<see cref="ProviderMembershipCache"/>).</summary>
    Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken);

    HashSet<string> GetPermissionsFromRoles(IEnumerable<string> roles);

    /// <summary>The providers the user is a member of, with their role in each, cached per user.</summary>
    Task<IReadOnlyDictionary<Guid, ProviderMemberRole>> GetProviderMembershipsAsync(Guid userId, CancellationToken cancellationToken);
}
