using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Domain.Cities;

/// <summary>
/// A city or municipality (a local self-government unit). It is hidden from parents until a category opens in it.
/// </summary>
public sealed class City : Entity
{
    private readonly List<CityCategory> _categories = [];

    private City()
    {
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public LocalizedText Name { get; private set; } = null!;

    /// <summary>The form used after "u" ("u Novom Sadu"). Required in every language before a category opens.</summary>
    public LocalizedText? LocativeName { get; private set; }

    public GeoCoordinate Center { get; private set; } = null!;

    public IReadOnlyCollection<CityCategory> Categories => _categories;

    public static Result<City> Create(string slug, LocalizedText name, GeoCoordinate center, LocalizedText? locativeName = null)
    {
        if (!UrlSlug.IsValid(slug))
        {
            return Result.Failure<City>(CityErrors.InvalidSlug);
        }

        return new City
        {
            Id = Guid.CreateVersion7(),
            Slug = slug,
            Name = name,
            LocativeName = locativeName,
            Center = center
        };
    }

    public Result OpenCategory(ProviderCategory category, DateTime utcNow)
    {
        if (!Enum.IsDefined(category))
        {
            return Result.Failure(CityErrors.UnknownCategory);
        }

        // English and Russian must be real translations: a Serbian locative ("Novom Sadu") reads wrong in either
        if (LocativeName is null || LocativeName.En is null || LocativeName.Ru is null)
        {
            return Result.Failure(CityErrors.LocativeNameRequired);
        }

        if (_categories.Any(c => c.Category == category))
        {
            return Result.Failure(CityErrors.CategoryAlreadyOpen);
        }

        _categories.Add(CityCategory.Create(Id, category, utcNow));

        return Result.Success();
    }

    public Result CloseCategory(ProviderCategory category)
    {
        CityCategory? open = _categories.SingleOrDefault(c => c.Category == category);
        if (open is null)
        {
            return Result.Failure(CityErrors.CategoryNotOpen);
        }

        _categories.Remove(open);

        return Result.Success();
    }
}
