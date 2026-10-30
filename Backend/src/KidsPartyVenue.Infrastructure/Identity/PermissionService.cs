using Dapper;
using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Users;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace KidsPartyVenue.Infrastructure.Identity;

internal sealed class PermissionService(
    IDbConnectionFactory dbConnectionFactory,
    ILogger<PermissionService> logger) : IPermissionService
{
    public async Task<Result<PermissionsResponse>> GetUserPermissionsAsync(string identityId, CancellationToken cancellationToken)
    {
        if (!Guid.TryParse(identityId, out Guid id))
        {
            logger.LogWarning("Failed to parse identityId: {IdentityId}", identityId);
            return Result.Failure<PermissionsResponse>(UserErrors.IdentityIdMalformed);
        }

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
            new CommandDefinition(sql, new { UserId = id }, cancellationToken: cancellationToken));

        if (user is null)
        {
            return Result.Failure<PermissionsResponse>(UserErrors.NotFound);
        }
        HashSet<string> rolePermissions = GetPermissionsFromRoles(user.Roles);
        return Result.Success(new PermissionsResponse(user.Id, rolePermissions));
    }

    public HashSet<string> GetPermissionsFromRoles(IEnumerable<string> roles)
    {
        return [.. roles.SelectMany(RolePermissions.For)];
    }

    private sealed class UserRow
    {
        public Guid Id { get; init; }
        public string[] Roles { get; init; } = [];
    }
}