using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Domain.Cities;

/// <summary>A district of a city, or a place nearby that parents search by ("Liman", "Petrovaradin").</summary>
public sealed class District : Entity
{
    private District()
    {
    }

    public Guid Id { get; private set; }

    public Guid CityId { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public LocalizedText Name { get; private set; } = null!;

    public static Result<District> Create(Guid cityId, string slug, LocalizedText name)
    {
        if (!UrlSlug.IsValid(slug))
        {
            return Result.Failure<District>(DistrictErrors.InvalidSlug);
        }

        return new District
        {
            Id = Guid.CreateVersion7(),
            CityId = cityId,
            Slug = slug,
            Name = name
        };
    }
}
