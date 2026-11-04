using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.Slugs;
using KidsPartyVenue.Infrastructure.Database.Seed;
using KidsPartyVenue.Infrastructure.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Amenities;

internal sealed class AmenityConfiguration : IEntityTypeConfiguration<Amenity>
{
    public void Configure(EntityTypeBuilder<Amenity> builder)
    {
        IReadOnlyList<SeedRow> seed = SeedCsv.Read("amenities.csv");

        builder.ToTable("amenities");
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Id).ValueGeneratedNever();

        builder.Property(a => a.Slug).HasMaxLength(UrlSlug.MaxLength);
        builder.HasIndex(a => a.Slug).IsUnique();

        builder.OwnsOne(a => a.Name, name =>
        {
            name.MapLocalizedText("name", LocalizedTextMapping.LookupNameMaxLength);
            name.HasData(seed.Select(row => new
            {
                AmenityId = row.Guid("id"),
                Sr = row.Text("name_sr"),
                En = row.OptionalText("name_en"),
                Ru = row.OptionalText("name_ru")
            }));
        });
        builder.Navigation(a => a.Name).IsRequired();

        builder.HasData(seed.Select(row => new
        {
            Id = row.Guid("id"),
            Slug = row.Text("slug"),
            SortOrder = row.Int("sort_order")
        }));
    }
}
