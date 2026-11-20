using System.Data.Common;
using Dapper;
using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.GetMyProviders;

// Not cached: the panel reads it once when it opens
internal sealed class GetMyProvidersQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    IUserContext userContext) : IQueryHandler<GetMyProvidersQuery, List<MyProviderResponse>>
{
    public async Task<Result<List<MyProviderResponse>>> Handle(GetMyProvidersQuery query, CancellationToken cancellationToken)
    {
        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        const string sql = $"""
            SELECT
                p.id AS {nameof(ProviderRow.Id)},
                p.slug AS {nameof(ProviderRow.Slug)},
                p.name AS {nameof(ProviderRow.Name)},
                p.status AS {nameof(ProviderRow.Status)},
                p.suspended_at_utc IS NOT NULL AS {nameof(ProviderRow.IsSuspended)},
                m.role AS {nameof(ProviderRow.Role)}
            FROM provider_members m
            JOIN providers p ON p.id = m.provider_id
            WHERE m.user_id = @UserId
            ORDER BY m.joined_at_utc, p.id
            """;

        IEnumerable<ProviderRow> rows = await connection.QueryAsync<ProviderRow>(
            new CommandDefinition(sql, new { userContext.UserId }, cancellationToken: cancellationToken));

        return rows
            .Select(r => new MyProviderResponse
            {
                Id = r.Id,
                Slug = r.Slug,
                Name = r.Name,
                Status = Enum.Parse<ProviderStatus>(r.Status),
                IsSuspended = r.IsSuspended,
                Role = Enum.Parse<ProviderMemberRole>(r.Role)
            })
            .ToList();
    }

    private sealed class ProviderRow
    {
        public Guid Id { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public bool IsSuspended { get; init; }
        public string Role { get; init; } = string.Empty;
    }
}
