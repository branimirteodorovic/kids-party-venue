using System.Data.Common;
using Dapper;
using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Users;
using Microsoft.Extensions.Logging;

namespace KidsPartyVenue.Infrastructure.Identity;

internal sealed class PermissionService(
    IDbConnectionFactory dbConnectionFactory,
    ICacheService cacheService,
    ILogger<PermissionService> logger) : IPermissionService
{
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(identityId, out Guid id))
        {
            logger.LogWarning("Failed to parse identityId: {IdentityId}", identityId);
            return Result.Failure<PermissionsResponse>(UserErrors.IdentityIdMalformed);
        }

        PermissionsResponse? permissions = await cacheService.GetOrCreateAsync(
            ProviderMembershipCache.PermissionsKey(id),
            async token => await ReadPermissionsAsync(id, token),
            ProviderMembershipCache.Expiration,
            [ProviderMembershipCache.UserTag(id)],
            cancellationToken);

        return permissions is null
            ? Result.Failure<PermissionsResponse>(UserErrors.NotFound)
            : Result.Success(permissions);
    }

    public HashSet<string> GetPermissionsFromRoles(IEnumerable<string> roles)
    {
        return [.. roles.SelectMany(RolePermissions.For)];
    }

    public async Task<IReadOnlyDictionary<Guid, ProviderMemberRole>> GetProviderMembershipsAsync(
        Guid userId,
        CancellationToken cancellationToken) =>
        await cacheService.GetOrCreateAsync(
            ProviderMembershipCache.MembershipsKey(userId),
            async token => await ReadMembershipsAsync(userId, token),
            ProviderMembershipCache.Expiration,
            [ProviderMembershipCache.UserTag(userId)],
            cancellationToken);

    private async Task<PermissionsResponse?> ReadPermissionsAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql = $"""
                SELECT
                    u.id AS {nameof(UserRow.Id)},
                    COALESCE(array_agg(r.name) FILTER (WHERE r.name IS NOT NULL), ARRAY[]::text[])
                        AS {nameof(UserRow.Roles)}
                FROM identity.users u
                LEFT JOIN identity.user_roles ur ON ur.user_id = u.id
                LEFT JOIN identity.roles r ON r.id = ur.role_id
                WHERE u.id = @UserId
                GROUP BY u.id
            """;

        UserRow? user = await dbConnection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return user is null ? null : new PermissionsResponse(user.Id, GetPermissionsFromRoles(user.Roles));
    }

    // A concrete dictionary, so the cache can serialize it
    private async Task<Dictionary<Guid, ProviderMemberRole>> ReadMembershipsAsync(Guid userId, CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql = $"""
                SELECT provider_id AS {nameof(MembershipRow.ProviderId)}, role AS {nameof(MembershipRow.Role)}
                FROM provider_members
                WHERE user_id = @UserId
            """;

        IEnumerable<MembershipRow> rows = await dbConnection.QueryAsync<MembershipRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        return rows.ToDictionary(r => r.ProviderId, r => Enum.Parse<ProviderMemberRole>(r.Role));
    }

    private sealed class UserRow
    {
        public Guid Id { get; init; }
        public string[] Roles { get; init; } = [];
    }

    private sealed class MembershipRow
    {
        public Guid ProviderId { get; init; }
        public string Role { get; init; } = string.Empty;
    }
}
