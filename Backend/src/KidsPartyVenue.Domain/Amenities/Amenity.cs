using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Domain.Amenities;

/// <summary>Something a venue has, that parents can filter by ("Parking", "Ball pit").</summary>
public sealed class Amenity : Entity
{
    private Amenity()
    {
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public LocalizedText Name { get; private set; } = null!;

    /// <summary>Position in lists; lower comes first.</summary>
    public int SortOrder { get; private set; }

    public static Result<Amenity> Create(string slug, LocalizedText name, int sortOrder)
    {
        if (!UrlSlug.IsValid(slug))
        {
            return Result.Failure<Amenity>(AmenityErrors.InvalidSlug);
        }

        return new Amenity
        {
            Id = Guid.CreateVersion7(),
            Slug = slug,
            Name = name,
            SortOrder = sortOrder
        };
    }
}
