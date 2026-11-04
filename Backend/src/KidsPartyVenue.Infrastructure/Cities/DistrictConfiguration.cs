using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Slugs;
using KidsPartyVenue.Infrastructure.Database.Seed;
using KidsPartyVenue.Infrastructure.Localization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Cities;

internal sealed class DistrictConfiguration : IEntityTypeConfiguration<District>
{
    public void Configure(EntityTypeBuilder<District> builder)
    {
        IReadOnlyList<SeedRow> seed = SeedCsv.Read("districts.csv");
        Dictionary<string, Guid> cityIds = SeedCsv.Read(CityConfiguration.SeedFile)
            .ToDictionary(row => row.Text("slug"), row => row.Guid("id"));

        builder.ToTable("districts");
        builder.HasKey(d => d.Id);
        builder.Property(d => d.Id).ValueGeneratedNever();

        builder.Property(d => d.Slug).HasMaxLength(UrlSlug.MaxLength);
        builder.HasIndex(d => new { d.CityId, d.Slug }).IsUnique();

        builder.OwnsOne(d => d.Name, name =>
        {
            name.MapLocalizedText("name", LocalizedTextMapping.LookupNameMaxLength);
            name.HasData(seed.Select(row => new
            {
                DistrictId = row.Guid("id"),
                Sr = row.Text("name_sr"),
                En = row.OptionalText("name_en"),
                Ru = row.OptionalText("name_ru")
            }));
        });
        builder.Navigation(d => d.Name).IsRequired();

        builder.HasOne<City>()
            .WithMany()
            .HasForeignKey(d => d.CityId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasData(seed.Select(row => new
        {
            Id = row.Guid("id"),
            CityId = cityIds[row.Text("city_slug")],
            Slug = row.Text("slug")
        }));
    }
}
