using KidsPartyVenue.Application.Abstractions.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace KidsPartyVenue.Infrastructure.Database;

internal sealed class EfTransaction(IDbContextTransaction transaction) : ITransaction
{
    public Task CommitAsync(CancellationToken cancellationToken = default) => transaction.CommitAsync(cancellationToken);

    public ValueTask DisposeAsync() => transaction.DisposeAsync();
}
