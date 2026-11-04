using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Lookups;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Geography;
using KidsPartyVenue.Infrastructure.Phones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Providers;

internal sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Slug).HasMaxLength(LookupSlug.MaxLength);
        builder.HasIndex(p => p.Slug).IsUnique();

        builder.Property(p => p.Name).HasMaxLength(Provider.NameMaxLength);
        builder.Property(p => p.Category).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Description).HasMaxLength(Provider.DescriptionMaxLength);
        builder.Property(p => p.Address).HasMaxLength(Provider.AddressMaxLength);

        builder.Property(p => p.Location)
            .HasConversion(new GeoCoordinateConverter())
            .HasColumnType(GeoCoordinateConverter.ColumnType);
        builder.HasIndex(p => p.Location).HasMethod("gist");

        builder.Property(p => p.Phone)
            .HasConversion<PhoneNumberConverter>()
            .HasMaxLength(PhoneNumber.MaxLength);

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        builder.Ignore(p => p.IsClaimed);

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(p => p.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<District>()
            .WithMany()
            .HasForeignKey(p => p.DistrictId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
