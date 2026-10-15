using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Identity;

internal sealed class RoleConfiguration : IEntityTypeConfiguration<Role>
{
    public void Configure(EntityTypeBuilder<Role> builder)
    {
        builder.HasData(
            new Role
            {
                Id = new Guid("0199a000-0000-7000-8000-000000000001"),
                Name = Role.Parent,
                NormalizedName = "PARENT",
                ConcurrencyStamp = "role-parent"
            },
            new Role
            {
                Id = new Guid("0199a000-0000-7000-8000-000000000002"),
                Name = Role.ProviderMember,
                NormalizedName = "PROVIDERMEMBER",
                ConcurrencyStamp = "role-providermember"
            },
            new Role
            {
                Id = new Guid("0199a000-0000-7000-8000-000000000003"),
                Name = Role.Admin,
                NormalizedName = "ADMIN",
                ConcurrencyStamp = "role-admin"
            });
    }
}
