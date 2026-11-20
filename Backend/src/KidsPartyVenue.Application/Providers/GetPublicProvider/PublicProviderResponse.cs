using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.GetPublicProvider;

public sealed record PublicProviderResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ProviderCategory Category { get; init; }
    public string? Description { get; init; }
    public string Address { get; init; } = string.Empty;

    /// <summary>With its slug, so the frontend can redirect to the canonical URL.</summary>
    public PublicCityResponse City { get; init; } = null!;

    public PublicDistrictResponse? District { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? Phone { get; init; }
    public bool IsClaimed { get; init; }
    public BookingPolicyResponse? BookingPolicy { get; init; }
    public VenueDetailsResponse? VenueDetails { get; init; }
    public List<PublicAmenityResponse> Amenities { get; init; } = [];
}
