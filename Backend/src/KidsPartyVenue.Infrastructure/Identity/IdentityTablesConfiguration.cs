using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace KidsPartyVenue.Infrastructure.Identity;

internal static class IdentityTablesConfiguration
{
    public static void Apply(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("users", "identity");
        modelBuilder.Entity<Role>().ToTable("roles", "identity");
        modelBuilder.Entity<IdentityUserRole<Guid>>().ToTable("user_roles", "identity");
        modelBuilder.Entity<IdentityUserClaim<Guid>>().ToTable("user_claims", "identity");
        modelBuilder.Entity<IdentityUserLogin<Guid>>().ToTable("user_logins", "identity");
        modelBuilder.Entity<IdentityUserToken<Guid>>().ToTable("user_tokens", "identity");
        modelBuilder.Entity<IdentityRoleClaim<Guid>>().ToTable("role_claims", "identity");
    }
}
