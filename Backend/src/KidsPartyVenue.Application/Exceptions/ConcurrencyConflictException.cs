namespace KidsPartyVenue.Application.Exceptions;

/// <summary>
/// Thrown by <see cref="Abstractions.Data.IUnitOfWork.SaveChangesAsync"/> when a row changed after it was loaded,
/// so handlers can turn it into a conflict without referencing EF Core.
/// </summary>
public sealed class ConcurrencyConflictException(Exception innerException)
    : Exception("The row was changed after it was loaded", innerException);
