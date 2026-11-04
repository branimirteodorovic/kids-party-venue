using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Geography;
using KidsPartyVenue.Domain.Phones;
using KidsPartyVenue.Domain.Slugs;

namespace KidsPartyVenue.Domain.Providers;

/// <summary>
/// A business parents book a party with, such as a venue. Created as a Draft, by an admin (unclaimed) or by its owner.
/// </summary>
public sealed class Provider : Entity
{
    public const int NameMaxLength = 150;
    public const int AddressMaxLength = 200;
    public const int DescriptionMaxLength = 5000;

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

        string trimmedName = name?.Trim() ?? string.Empty;
        if (trimmedName.Length == 0)
        {
            return Result.Failure<Provider>(ProviderErrors.NameRequired);
        }

        if (trimmedName.Length > NameMaxLength)
        {
            return Result.Failure<Provider>(ProviderErrors.NameTooLong);
        }

        if (!Enum.IsDefined(category))
        {
            return Result.Failure<Provider>(ProviderErrors.UnknownCategory);
        }

        string? trimmedDescription = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        if (trimmedDescription?.Length > DescriptionMaxLength)
        {
            return Result.Failure<Provider>(ProviderErrors.DescriptionTooLong);
        }

        string trimmedAddress = address?.Trim() ?? string.Empty;
        if (trimmedAddress.Length == 0)
        {
            return Result.Failure<Provider>(ProviderErrors.AddressRequired);
        }

        if (trimmedAddress.Length > AddressMaxLength)
        {
            return Result.Failure<Provider>(ProviderErrors.AddressTooLong);
        }

        if (district is not null && district.CityId != cityId)
        {
            return Result.Failure<Provider>(ProviderErrors.DistrictNotInCity);
        }

        if (district is null && cityHasDistricts)
        {
            return Result.Failure<Provider>(ProviderErrors.DistrictRequired);
        }

        // GeoCoordinate accepts swapped latitude and longitude; the bounding box doesn't
        if (!SerbiaBounds.Contains(location))
        {
            return Result.Failure<Provider>(ProviderErrors.LocationOutsideSerbia);
        }

        return new Provider
        {
            Id = Guid.CreateVersion7(),
            Slug = slug,
            Name = trimmedName,
            Category = category,
            Description = trimmedDescription,
            Address = trimmedAddress,
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
}
