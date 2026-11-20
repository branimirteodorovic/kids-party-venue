using System.Data.Common;
using Dapper;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.Application.Providers.GetProviderForEditing;

internal sealed class GetProviderForEditingQueryHandler(IDbConnectionFactory dbConnectionFactory)
    : IQueryHandler<GetProviderForEditingQuery, ProviderForEditingResponse>
{
    // xmin is the row version the update commands compare against; it's cast through text because xid has no cast to bigint
    private const string Sql = $"""
        SELECT
            id AS {nameof(ProviderRow.Id)},
            slug AS {nameof(ProviderRow.Slug)},
            name AS {nameof(ProviderRow.Name)},
            category AS {nameof(ProviderRow.Category)},
            description AS {nameof(ProviderRow.Description)},
            address AS {nameof(ProviderRow.Address)},
            city_id AS {nameof(ProviderRow.CityId)},
            district_id AS {nameof(ProviderRow.DistrictId)},
            ST_Y(location::geometry) AS {nameof(ProviderRow.Latitude)},
            ST_X(location::geometry) AS {nameof(ProviderRow.Longitude)},
            phone AS {nameof(ProviderRow.Phone)},
            status AS {nameof(ProviderRow.Status)},
            claimed_at_utc IS NOT NULL AS {nameof(ProviderRow.IsClaimed)},
            review_notes AS {nameof(ProviderRow.ReviewNotes)},
            submitted_at_utc AS {nameof(ProviderRow.SubmittedAtUtc)},
            reviewed_at_utc AS {nameof(ProviderRow.ReviewedAtUtc)},
            suspended_at_utc AS {nameof(ProviderRow.SuspendedAtUtc)},
            suspension_reason AS {nameof(ProviderRow.SuspensionReason)},
            booking_deposit_type AS {nameof(ProviderRow.DepositType)},
            booking_deposit_value AS {nameof(ProviderRow.DepositValue)},
            booking_deposit_due_days AS {nameof(ProviderRow.DepositDueDays)},
            booking_free_cancellation_days AS {nameof(ProviderRow.FreeCancellationDays)},
            venue_max_children AS {nameof(ProviderRow.MaxChildren)},
            venue_max_adults AS {nameof(ProviderRow.MaxAdults)},
            venue_min_age AS {nameof(ProviderRow.MinAge)},
            venue_max_age AS {nameof(ProviderRow.MaxAge)},
            venue_outside_catering AS {nameof(ProviderRow.OutsideCatering)},
            venue_outside_cake AS {nameof(ProviderRow.OutsideCake)},
            venue_food_service AS {nameof(ProviderRow.FoodService)},
            venue_has_own_animator AS {nameof(ProviderRow.HasOwnAnimator)},
            venue_has_own_photographer AS {nameof(ProviderRow.HasOwnPhotographer)},
            venue_has_smoking_area AS {nameof(ProviderRow.HasSmokingArea)},
            xmin::text::bigint AS {nameof(ProviderRow.Version)}
        FROM providers
        WHERE id = @ProviderId;

        SELECT amenity_id
        FROM provider_amenities
        WHERE provider_id = @ProviderId
        ORDER BY amenity_id;
        """;

    public async Task<Result<ProviderForEditingResponse>> Handle(GetProviderForEditingQuery query, CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbConnectionFactory.OpenConnectionAsync();

        await using SqlMapper.GridReader grid = await dbConnection.QueryMultipleAsync(
            new CommandDefinition(Sql, new { query.ProviderId }, cancellationToken: cancellationToken));

        ProviderRow? row = await grid.ReadSingleOrDefaultAsync<ProviderRow>();
        if (row is null)
        {
            return Result.Failure<ProviderForEditingResponse>(ProviderErrors.NotFound);
        }

        List<Guid> amenityIds = [.. await grid.ReadAsync<Guid>()];

        return new ProviderForEditingResponse
        {
            Id = row.Id,
            Slug = row.Slug,
            Name = row.Name,
            Category = Enum.Parse<ProviderCategory>(row.Category),
            Description = row.Description,
            Address = row.Address,
            CityId = row.CityId,
            DistrictId = row.DistrictId,
            Latitude = row.Latitude,
            Longitude = row.Longitude,
            Phone = row.Phone,
            Status = Enum.Parse<ProviderStatus>(row.Status),
            IsClaimed = row.IsClaimed,
            ReviewNotes = row.ReviewNotes,
            SubmittedAtUtc = row.SubmittedAtUtc,
            ReviewedAtUtc = row.ReviewedAtUtc,
            SuspendedAtUtc = row.SuspendedAtUtc,
            SuspensionReason = row.SuspensionReason,
            BookingPolicy = row.DepositType is null
                ? null
                : new BookingPolicyResponse(
                    Enum.Parse<DepositType>(row.DepositType),
                    row.DepositValue,
                    row.DepositDueDays,
                    row.FreeCancellationDays!.Value),
            VenueDetails = row.MaxChildren is null
                ? null
                : new VenueDetailsResponse(
                    row.MaxChildren.Value,
                    row.MaxAdults,
                    row.MinAge!.Value,
                    row.MaxAge!.Value,
                    Enum.Parse<OutsideFoodRule>(row.OutsideCatering!),
                    Enum.Parse<OutsideFoodRule>(row.OutsideCake!),
                    Enum.Parse<FoodService>(row.FoodService!),
                    row.HasOwnAnimator!.Value,
                    row.HasOwnPhotographer!.Value,
                    row.HasSmokingArea!.Value),
            AmenityIds = amenityIds,
            Version = (uint)row.Version
        };
    }

    // The owned types share the providers row, and their check constraints make each set all-null or complete
    private sealed class ProviderRow
    {
        public Guid Id { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string Address { get; init; } = string.Empty;
        public Guid CityId { get; init; }
        public Guid? DistrictId { get; init; }
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public string? Phone { get; init; }
        public string Status { get; init; } = string.Empty;
        public bool IsClaimed { get; init; }
        public string? ReviewNotes { get; init; }
        public DateTime? SubmittedAtUtc { get; init; }
        public DateTime? ReviewedAtUtc { get; init; }
        public DateTime? SuspendedAtUtc { get; init; }
        public string? SuspensionReason { get; init; }
        public string? DepositType { get; init; }
        public decimal? DepositValue { get; init; }
        public int? DepositDueDays { get; init; }
        public int? FreeCancellationDays { get; init; }
        public int? MaxChildren { get; init; }
        public int? MaxAdults { get; init; }
        public int? MinAge { get; init; }
        public int? MaxAge { get; init; }
        public string? OutsideCatering { get; init; }
        public string? OutsideCake { get; init; }
        public string? FoodService { get; init; }
        public bool? HasOwnAnimator { get; init; }
        public bool? HasOwnPhotographer { get; init; }
        public bool? HasSmokingArea { get; init; }
        public long Version { get; init; }
    }
}
