using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Identity;

internal sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("refresh_tokens", "identity");
        builder.HasKey(t => t.Id);
        builder.Property(t => t.Version).IsRowVersion();
        builder.HasIndex(t => t.TokenHash).IsUnique();
        builder.Property(t => t.TokenHash).HasMaxLength(64);
        builder.HasIndex(t => t.FamilyId);
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
