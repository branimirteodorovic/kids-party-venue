using KidsPartyVenue.Application.Data;
using Microsoft.Data.SqlClient;
using System.Data.Common;

namespace KidsPartyVenue.Infrastructure.Data;

internal sealed class DbConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public async ValueTask<DbConnection> OpenConnectionAsync()
    {
        return new SqlConnection(connectionString);
    }
}
