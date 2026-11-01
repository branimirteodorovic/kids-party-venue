using KidsPartyVenue.Domain.Cities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace KidsPartyVenue.Infrastructure.Cities;

internal sealed class CityCategoryConfiguration : IEntityTypeConfiguration<CityCategory>
{
    public void Configure(EntityTypeBuilder<CityCategory> builder)
    {
        builder.ToTable("city_categories");
        builder.HasKey(c => new { c.CityId, c.Category });
        builder.Property(c => c.Category).HasConversion<string>().HasMaxLength(20);
    }
}
