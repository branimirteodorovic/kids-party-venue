using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Providers.GetProviderForEditing;

public sealed record ProviderForEditingResponse
{
    public Guid Id { get; init; }
    public string Slug { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public ProviderCategory Category { get; init; }
    public string? Description { get; init; }
    public string Address { get; init; } = string.Empty;
    public Guid CityId { get; init; }
    public Guid? DistrictId { get; init; }
    public double Latitude { get; init; }
    public double Longitude { get; init; }
    public string? Phone { get; init; }
    public ProviderStatus Status { get; init; }
    public bool IsClaimed { get; init; }

    /// <summary>What the admin asked to change at the last review; null once approved.</summary>
    public string? ReviewNotes { get; init; }

    public DateTime? SubmittedAtUtc { get; init; }
    public DateTime? ReviewedAtUtc { get; init; }

    /// <summary>Set while an admin has suspended the profile, together with the reason.</summary>
    public DateTime? SuspendedAtUtc { get; init; }

    public string? SuspensionReason { get; init; }

    /// <summary>Null until the provider sets its terms.</summary>
    public BookingPolicyResponse? BookingPolicy { get; init; }

    /// <summary>Null until the venue sets them.</summary>
    public VenueDetailsResponse? VenueDetails { get; init; }

    public List<Guid> AmenityIds { get; init; } = [];

    public uint Version { get; init; }
}
