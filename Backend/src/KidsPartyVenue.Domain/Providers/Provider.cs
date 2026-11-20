using KidsPartyVenue.Domain.Amenities;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Slugs;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.Domain.Providers;

/// <summary>
/// A business parents book a party with, such as a venue. Created as a Draft, by an admin (unclaimed) or by its owner.
/// </summary>
public sealed class Provider : Entity
{
    public const int NameMaxLength = 150;
    public const int AddressMaxLength = 200;
    public const int DescriptionMaxLength = 5000;

    private readonly List<ProviderAmenity> _amenities = [];
    private readonly List<ProviderMember> _members = [];

    private Provider()
    {
    }

    public Guid Id { get; private set; }

    public string Slug { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public ProviderCategory Category { get; private set; }

    /// <summary>Written by the provider in any language and shown as written.</summary>
    public string? Description { get; private set; }

    /// <summary>The street and number, without the city.</summary>
    public string Address { get; private set; } = string.Empty;

    public Guid CityId { get; private set; }

    /// <summary>Required in a city that has districts when the place is set; null in a city without any.</summary>
    public Guid? DistrictId { get; private set; }

    public GeoCoordinate Location { get; private set; } = null!;

    public PhoneNumber? Phone { get; private set; }

    /// <summary>Null until the provider sets its terms; required before review.</summary>
    public BookingPolicy? BookingPolicy { get; private set; }

    /// <summary>Venues only; null until the venue sets them, and required before review.</summary>
    public VenueDetails? VenueDetails { get; private set; }

    /// <summary>Venues only until P2.11; empty until the provider marks some.</summary>
    public IReadOnlyCollection<ProviderAmenity> Amenities => _amenities;

    /// <summary>The users who run the profile; empty while it is unclaimed, and never without an owner once claimed.</summary>
    public IReadOnlyCollection<ProviderMember> Members => _members;

    public ProviderStatus Status { get; private set; }

    /// <summary>When an owner took over the profile; null while it is unclaimed.</summary>
    public DateTime? ClaimedAtUtc { get; private set; }

    /// <summary>Set while an admin has suspended the profile. The review status stays as it was.</summary>
    public DateTime? SuspendedAtUtc { get; private set; }

    public DateTime CreatedAtUtc { get; private set; }

    public DateTime UpdatedAtUtc { get; private set; }

    public bool IsClaimed => ClaimedAtUtc is not null;

    public static Result<Provider> Create(
        string slug,
        string name,
        ProviderCategory category,
        string? description,
        string address,
        Guid cityId,
        District? district,
        bool cityHasDistricts,
        GeoCoordinate location,
        PhoneNumber? phone,
        bool isClaimed,
        DateTime utcNow)
    {
        if (!UrlSlug.IsValid(slug))
        {
            return Result.Failure<Provider>(ProviderErrors.InvalidSlug);
        }

        if (ProviderSlug.IsReserved(slug))
        {
            return Result.Failure<Provider>(ProviderErrors.ReservedSlug);
        }

        if (!Enum.IsDefined(category))
        {
            return Result.Failure<Provider>(ProviderErrors.UnknownCategory);
        }

        Result<BasicInfo> basicInfo = CheckBasicInfo(name, description, address, cityId, district, cityHasDistricts, location);
        if (basicInfo.IsFailure)
        {
            return Result.Failure<Provider>(basicInfo.Error);
        }

        return new Provider
        {
            Id = Guid.CreateVersion7(),
            Slug = slug,
            Name = basicInfo.Value.Name,
            Category = category,
            Description = basicInfo.Value.Description,
            Address = basicInfo.Value.Address,
            CityId = cityId,
            DistrictId = district?.Id,
            Location = location,
            Phone = phone,
            Status = ProviderStatus.Draft,
            ClaimedAtUtc = isClaimed ? utcNow : null,
            CreatedAtUtc = utcNow,
            UpdatedAtUtc = utcNow
        };
    }

    /// <summary>
    /// Replaces the name, description and place, with the same checks as <see cref="Create"/>. The slug stays as it
    /// was (1.4), even when the name or the city changes.
    /// </summary>
    public Result ChangeBasicInfo(
        string name,
        string? description,
        string address,
        Guid cityId,
        District? district,
        bool cityHasDistricts,
        GeoCoordinate location,
        PhoneNumber? phone,
        DateTime utcNow)
    {
        Result<BasicInfo> basicInfo = CheckBasicInfo(name, description, address, cityId, district, cityHasDistricts, location);
        if (basicInfo.IsFailure)
        {
            return Result.Failure(basicInfo.Error);
        }

        Name = basicInfo.Value.Name;
        Description = basicInfo.Value.Description;
        Address = basicInfo.Value.Address;
        CityId = cityId;
        DistrictId = district?.Id;
        Location = location;
        Phone = phone;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    public void ChangeBookingPolicy(BookingPolicy policy, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(policy);

        BookingPolicy = policy;
        UpdatedAtUtc = utcNow;
    }

    public Result ChangeVenueDetails(VenueDetails details, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(details);

        if (Category != ProviderCategory.Venue)
        {
            return Result.Failure(ProviderErrors.VenueDetailsNotForCategory);
        }

        VenueDetails = details;
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>Replaces the set of amenities, keeping the ones that stay so their rows aren't deleted and inserted again.</summary>
    public Result ChangeAmenities(IReadOnlyCollection<Amenity> amenities, DateTime utcNow)
    {
        ArgumentNullException.ThrowIfNull(amenities);

        if (Category != ProviderCategory.Venue)
        {
            return Result.Failure(ProviderErrors.AmenitiesNotForCategory);
        }

        HashSet<Guid> wanted = amenities.Select(a => a.Id).ToHashSet();
        int removed = _amenities.RemoveAll(a => !wanted.Contains(a.AmenityId));

        HashSet<Guid> kept = _amenities.Select(a => a.AmenityId).ToHashSet();
        List<Guid> added = wanted.Where(id => !kept.Contains(id)).ToList();
        _amenities.AddRange(added.Select(id => ProviderAmenity.Create(Id, id)));

        if (removed > 0 || added.Count > 0)
        {
            UpdatedAtUtc = utcNow;
        }

        return Result.Success();
    }

    /// <summary>
    /// Adds a user to the people who run the provider. It sets <see cref="UpdatedAtUtc"/>, so the row version moves and a
    /// concurrent membership change is caught.
    /// </summary>
    public Result AddMember(Guid userId, ProviderMemberRole role, DateTime utcNow)
    {
        if (!Enum.IsDefined(role))
        {
            return Result.Failure(ProviderErrors.UnknownMemberRole);
        }

        if (_members.Exists(m => m.UserId == userId))
        {
            return Result.Failure(ProviderErrors.AlreadyMember);
        }

        _members.Add(ProviderMember.Create(Id, userId, role, utcNow));
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    /// <summary>
    /// Removes a member, unless they are the only owner. It sets <see cref="UpdatedAtUtc"/>, so two owners removing each
    /// other at the same time can't both succeed and leave the provider without one.
    /// </summary>
    public Result RemoveMember(Guid userId, DateTime utcNow)
    {
        ProviderMember? member = _members.Find(m => m.UserId == userId);
        if (member is null)
        {
            return Result.Failure(ProviderErrors.MemberNotFound);
        }

        if (member.Role == ProviderMemberRole.Owner && _members.Count(m => m.Role == ProviderMemberRole.Owner) == 1)
        {
            return Result.Failure(ProviderErrors.LastOwner);
        }

        _members.Remove(member);
        UpdatedAtUtc = utcNow;

        return Result.Success();
    }

    private static Result<BasicInfo> CheckBasicInfo(
        string name,
        string? description,
        string address,
        Guid cityId,
        District? district,
        bool cityHasDistricts,
        GeoCoordinate location)
    {
        string trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0)
        {
            return Result.Failure<BasicInfo>(ProviderErrors.NameRequired);
        }

        if (trimmedName.Length > NameMaxLength)
        {
            return Result.Failure<BasicInfo>(ProviderErrors.NameTooLong);
        }

        string? trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (trimmedDescription?.Length > DescriptionMaxLength)
        {
            return Result.Failure<BasicInfo>(ProviderErrors.DescriptionTooLong);
        }

        string trimmedAddress = address?.Trim() ?? string.Empty;
        if (trimmedAddress.Length == 0)
        {
            return Result.Failure<BasicInfo>(ProviderErrors.AddressRequired);
        }

        if (trimmedAddress.Length > AddressMaxLength)
        {
            return Result.Failure<BasicInfo>(ProviderErrors.AddressTooLong);
        }

        if (district is not null && district.CityId != cityId)
        {
            return Result.Failure<BasicInfo>(ProviderErrors.DistrictNotInCity);
        }

        if (district is null && cityHasDistricts)
        {
            return Result.Failure<BasicInfo>(ProviderErrors.DistrictRequired);
        }

        // GeoCoordinate accepts swapped latitude and longitude; the bounding box doesn't
        if (!SerbiaBounds.Contains(location))
        {
            return Result.Failure<BasicInfo>(ProviderErrors.LocationOutsideSerbia);
        }

        return new BasicInfo(trimmedName, trimmedDescription, trimmedAddress);
    }

    private sealed record BasicInfo(string Name, string? Description, string Address);
}
