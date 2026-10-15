using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Database;

public class KidsPartyVenueDbContext(DbContextOptions<KidsPartyVenueDbContext> options) : IdentityDbContext<User, Role, Guid>(options), IUnitOfWork
{
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        IdentityTablesConfiguration.Apply(modelBuilder);

        modelBuilder.HasPostgresExtension("postgis");

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(KidsPartyVenueDbContext).Assembly);
    }
}
