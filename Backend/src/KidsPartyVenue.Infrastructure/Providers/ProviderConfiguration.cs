using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Slugs;
using KidsPartyVenue.Infrastructure.Geography;
using KidsPartyVenue.Infrastructure.Phones;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Providers;

internal sealed class ProviderConfiguration : IEntityTypeConfiguration<Provider>
{
    public void Configure(EntityTypeBuilder<Provider> builder)
    {
        builder.ToTable("providers", table =>
        {
            table.HasCheckConstraint(
                "ck_providers_booking_policy_complete",
                """
                (booking_deposit_type IS NULL AND booking_deposit_value IS NULL
                    AND booking_deposit_due_days IS NULL AND booking_free_cancellation_days IS NULL)
                OR (booking_deposit_type IS NOT NULL AND booking_free_cancellation_days IS NOT NULL)
                """);
            table.HasCheckConstraint(
                "ck_providers_booking_deposit",
                """
                booking_deposit_type IS NULL
                OR (booking_deposit_type = 'None' AND booking_deposit_value IS NULL AND booking_deposit_due_days IS NULL)
                OR (booking_deposit_type IN ('Percentage', 'FixedAmount')
                    AND booking_deposit_value IS NOT NULL AND booking_deposit_due_days IS NOT NULL)
                """);
        });
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Slug).HasMaxLength(UrlSlug.MaxLength);
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

        // Owned, so the policy's columns live in this table and it is null until set
        builder.OwnsOne(p => p.BookingPolicy, policy =>
        {
            policy.Property(b => b.DepositType)
                .HasColumnName("booking_deposit_type")
                .HasConversion<string>()
                .HasMaxLength(20);
            policy.Property(b => b.DepositValue).HasColumnName("booking_deposit_value").HasPrecision(12, 2);
            policy.Property(b => b.DepositDueDays).HasColumnName("booking_deposit_due_days");
            policy.Property(b => b.FreeCancellationDays).HasColumnName("booking_free_cancellation_days");
        });

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
