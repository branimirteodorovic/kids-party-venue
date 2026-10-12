using System.Data.Common;

namespace KidsPartyVenue.Application.Data;

public interface IDbConnectionFactory
{
    ValueTask<DbConnection> OpenConnectionAsync();
}
