using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Lookups;
using KidsPartyVenue.Infrastructure.Database.Seed;
using KidsPartyVenue.Infrastructure.Geography;
using KidsPartyVenue.Infrastructure.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Cities;

internal sealed class CityConfiguration : IEntityTypeConfiguration<City>
{
    public const string SeedFile = "cities.csv";

    public void Configure(EntityTypeBuilder<City> builder)
    {
        IReadOnlyList<SeedRow> seed = SeedCsv.Read(SeedFile);

        builder.ToTable("cities");
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever();

        builder.Property(c => c.Slug).HasMaxLength(LookupSlug.MaxLength);
        builder.HasIndex(c => c.Slug).IsUnique();

        builder.OwnsOne(c => c.Name, name =>
        {
            name.MapLocalizedText("name", LocalizedTextMapping.LookupNameMaxLength);
            name.HasData(seed.Select(row => new
            {
                CityId = row.Guid("id"),
                Sr = row.Text("name_sr"),
                En = row.OptionalText("name_en"),
                Ru = row.OptionalText("name_ru")
            }));
        });
        builder.Navigation(c => c.Name).IsRequired();

        builder.OwnsOne(c => c.LocativeName, locative =>
        {
            locative.MapLocalizedText("locative_name", LocalizedTextMapping.LookupNameMaxLength);
            locative.HasData(seed.Where(row => row.OptionalText("locative_name_sr") is not null).Select(row => new
            {
                CityId = row.Guid("id"),
                Sr = row.Text("locative_name_sr"),
                En = row.OptionalText("locative_name_en"),
                Ru = row.OptionalText("locative_name_ru")
            }));
        });

        builder.Property(c => c.Center)
            .HasConversion(new GeoCoordinateConverter())
            .HasColumnType(GeoCoordinateConverter.ColumnType);
        builder.HasIndex(c => c.Center).HasMethod("gist");

        builder.HasMany(c => c.Categories)
            .WithOne()
            .HasForeignKey(c => c.CityId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasData(seed.Select(row => new
        {
            Id = row.Guid("id"),
            Slug = row.Text("slug"),
            Center = GeoCoordinate.Create(row.Double("latitude"), row.Double("longitude")).Value
        }));
    }
}
