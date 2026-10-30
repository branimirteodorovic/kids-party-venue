namespace KidsPartyVenue.Application.Abstractions.Data;

/// <summary>A database transaction. Disposing it without <see cref="CommitAsync"/> rolls it back.</summary>
public interface ITransaction : IAsyncDisposable
{
    Task CommitAsync(CancellationToken cancellationToken = default);
}
