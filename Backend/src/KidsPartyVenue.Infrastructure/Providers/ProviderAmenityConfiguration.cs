using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.Providers;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Providers;

internal sealed class ProviderAmenityConfiguration : IEntityTypeConfiguration<ProviderAmenity>
{
    public void Configure(EntityTypeBuilder<ProviderAmenity> builder)
    {
        builder.ToTable("provider_amenities");
        builder.HasKey(a => new { a.ProviderId, a.AmenityId });

        // Restrict, so an amenity in use can't disappear from providers; 7.6 decides how to retire one
        builder.HasOne<Amenity>()
            .WithMany()
            .HasForeignKey(a => a.AmenityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Serves the restrict check and search by amenity (3.5); the primary key covers lookups by provider
        builder.HasIndex(a => a.AmenityId);
    }
}
