using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Domain.Cities;

/// <summary>A category that is open in a city: its providers are shown to parents there.</summary>
public sealed class CityCategory : Entity
{
    private CityCategory()
    {
    }

    public Guid CityId { get; private set; }

    public ProviderCategory Category { get; private set; }

    public DateTime OpenedAtUtc { get; private set; }

    internal static CityCategory Create(Guid cityId, ProviderCategory category, DateTime openedAtUtc) =>
        new()
        {
            CityId = cityId,
            Category = category,
            OpenedAtUtc = openedAtUtc
        };
}
