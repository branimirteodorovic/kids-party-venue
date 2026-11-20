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
    /// <summary>A shadow property mapped to Postgres <c>xmin</c>, so Domain doesn't carry a persistence detail.</summary>
    internal const string VersionProperty = "Version";

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
            table.HasCheckConstraint(
                "ck_providers_venue_details_complete",
                """
                (venue_max_children IS NULL AND venue_max_adults IS NULL AND venue_min_age IS NULL
                    AND venue_max_age IS NULL AND venue_outside_catering IS NULL AND venue_outside_cake IS NULL
                    AND venue_food_service IS NULL AND venue_has_own_animator IS NULL
                    AND venue_has_own_photographer IS NULL AND venue_has_smoking_area IS NULL)
                OR (venue_max_children IS NOT NULL AND venue_min_age IS NOT NULL AND venue_max_age IS NOT NULL
                    AND venue_outside_catering IS NOT NULL AND venue_outside_cake IS NOT NULL
                    AND venue_food_service IS NOT NULL AND venue_has_own_animator IS NOT NULL
                    AND venue_has_own_photographer IS NOT NULL AND venue_has_smoking_area IS NOT NULL)
                """);
            table.HasCheckConstraint("ck_providers_venue_age_range", "venue_min_age <= venue_max_age");
            table.HasCheckConstraint(
                "ck_providers_venue_details_category",
                "category = 'Venue' OR venue_max_children IS NULL");
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

        // Owned like the booking policy; the complete check makes venue_max_children stand for the whole set
        builder.OwnsOne(p => p.VenueDetails, venue =>
        {
            venue.Property(v => v.MaxChildren).HasColumnName("venue_max_children");
            venue.Property(v => v.MaxAdults).HasColumnName("venue_max_adults");
            venue.Property(v => v.MinAge).HasColumnName("venue_min_age");
            venue.Property(v => v.MaxAge).HasColumnName("venue_max_age");
            venue.Property(v => v.OutsideCatering)
                .HasColumnName("venue_outside_catering")
                .HasConversion<string>()
                .HasMaxLength(20);
            venue.Property(v => v.OutsideCake)
                .HasColumnName("venue_outside_cake")
                .HasConversion<string>()
                .HasMaxLength(20);
            venue.Property(v => v.FoodService)
                .HasColumnName("venue_food_service")
                .HasConversion<string>()
                .HasMaxLength(20);
            venue.Property(v => v.HasOwnAnimator).HasColumnName("venue_has_own_animator");
            venue.Property(v => v.HasOwnPhotographer).HasColumnName("venue_has_own_photographer");
            venue.Property(v => v.HasSmokingArea).HasColumnName("venue_has_smoking_area");
        });

        builder.Property(p => p.Status).HasConversion<string>().HasMaxLength(20);

        // Optimistic concurrency for profile edits (1.9)
        builder.Property<uint>(VersionProperty).IsRowVersion();

        builder.Ignore(p => p.IsClaimed);

        // Part of the aggregate, so they always load with it and ChangeAmenities diffs against the real set
        builder.HasMany(p => p.Amenities)
            .WithOne()
            .HasForeignKey(a => a.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Amenities).AutoInclude();

        // Part of the aggregate too, so last-owner protection always checks the real set
        builder.HasMany(p => p.Members)
            .WithOne()
            .HasForeignKey(m => m.ProviderId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.Navigation(p => p.Members).AutoInclude();

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
