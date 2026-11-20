using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Providers;

internal sealed class ProviderMemberConfiguration : IEntityTypeConfiguration<ProviderMember>
{
    public void Configure(EntityTypeBuilder<ProviderMember> builder)
    {
        builder.ToTable("provider_members");
        builder.HasKey(m => new { m.ProviderId, m.UserId });

        builder.Property(m => m.Role).HasConversion<string>().HasMaxLength(20);

        // Restrict, so deleting a user can't silently remove a provider's last owner
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Serves the per-user membership lookup behind the ProviderMember policy; the primary key covers lookups by provider
        builder.HasIndex(m => m.UserId);
    }
}
