using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Exceptions;
using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Users;
using KidsPartyVenue.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Database;

public class KidsPartyVenueDbContext(DbContextOptions<KidsPartyVenueDbContext> options) : IdentityDbContext<User, Role, Guid>(options), IUnitOfWork
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    public DbSet<UserProfile> UserProfiles { get; set; }

    public DbSet<City> Cities { get; set; }

    public DbSet<District> Districts { get; set; }

    public DbSet<Amenity> Amenities { get; set; }

    public DbSet<Provider> Providers { get; set; }

    /// <summary>
    /// The save handlers call. A row that changed after it was loaded becomes <see cref="ConcurrencyConflictException"/>,
    /// so Application never sees EF types; code here that saves the context directly still gets EF's exception.
    /// </summary>
    async Task<int> IUnitOfWork.SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            return await SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException exception)
        {
            throw new ConcurrencyConflictException(exception);
        }
    }

    public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default) =>
        new EfTransaction(await Database.BeginTransactionAsync(cancellationToken));

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        IdentityTablesConfiguration.Apply(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KidsPartyVenueDbContext).Assembly);
    }
}
