using KidsPartyVenue.Application.Data;
using Npgsql;
using System.Data.Common;

namespace KidsPartyVenue.Infrastructure.Data;

internal sealed class DbConnectionFactory(NpgsqlDataSource dataSource) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return await dataSource.OpenConnectionAsync();
    }
}
