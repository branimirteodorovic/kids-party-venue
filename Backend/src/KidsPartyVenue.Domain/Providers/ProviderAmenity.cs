namespace KidsPartyVenue.Domain.Providers;

/// <summary>An amenity a provider has, referred to by ID so the amenity itself stays outside the aggregate.</summary>
public sealed class ProviderAmenity : Entity
{
    private ProviderAmenity()
    {
    }

    public Guid ProviderId { get; private set; }

    public Guid AmenityId { get; private set; }

    internal static ProviderAmenity Create(Guid providerId, Guid amenityId) =>
        new()
        {
            ProviderId = providerId,
            AmenityId = amenityId
        };
}
