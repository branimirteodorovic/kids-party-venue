using Dapper;
using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Authorization;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Users;
using System.Data.Common;

namespace KidsPartyVenue.Application.Users.GetCurrentUser;

internal sealed class GetCurrentUserQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext,
    IPermissionService permissionService) : IQueryHandler<GetCurrentUserQuery, CurrentUserResponse>
{
    public async Task<Result<CurrentUserResponse>> Handle(GetCurrentUserQuery query, CancellationToken cancellationToken)
    {
        Guid userId = userContext.UserId;

        await using DbConnection dbConnection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql = $"""
                SELECT 
                    u.id AS {nameof(UserRow.Id)},
                    u.email AS {nameof(UserRow.Email)},
                    u.full_name AS {nameof(UserRow.FullName)},
                    COALESCE(array_agg(r.name) FILTER (WHERE r.name IS NOT NULL), ARRAY[]::text[])
                        AS {nameof(UserRow.Roles)}
                FROM identity.users u
                LEFT JOIN identity.user_roles ur ON ur.user_id = u.id
                LEFT JOIN identity.roles r ON r.id = ur.role_id
                WHERE u.id = @UserId
                GROUP BY u.id, u.email, u.full_name
            """;

        UserRow? user = await dbConnection.QuerySingleOrDefaultAsync<UserRow>(
            new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));

        if (user is null)
        {
            return Result.Failure<CurrentUserResponse>(UserErrors.NotFound);
        }

        HashSet<string> permissions = permissionService.GetPermissionsFromRoles(user.Roles);

        return Result.Success(new CurrentUserResponse
        {
            Id = userId,
            Email = user.Email,
            FullName = user.FullName,
            Roles = [.. user.Roles],
            Permissions = [.. permissions]
        });
    }

    private sealed class UserRow
    {
        public Guid Id { get; init; }
        public string Email { get; init; } = string.Empty;
        public string FullName { get; init; } = string.Empty;
        public string[] Roles { get; init; } = [];
    }
}
