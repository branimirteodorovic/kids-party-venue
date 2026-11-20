using System.Data.Common;
using Dapper;
using KidsPartyVenue.Application.Abstractions.Localization;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Application.Lookups;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.BookingPolicies;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Domain.Slugs;
using KidsPartyVenue.Domain.Venues;

namespace KidsPartyVenue.Application.Providers.GetPublicProvider;

internal sealed class GetPublicProviderQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ILanguageContext languageContext,
    ICacheService cacheService) : IQueryHandler<GetPublicProviderQuery, PublicProviderResponse>
{
    // Any status: the page query below decides visibility, and a slug's provider never changes
    private const string IdSql = "SELECT id FROM providers WHERE slug = @Slug;";

    private const string PageSql = $"""
        SELECT
            p.id AS {nameof(ProviderRow.Id)},
            p.slug AS {nameof(ProviderRow.Slug)},
            p.name AS {nameof(ProviderRow.Name)},
            p.category AS {nameof(ProviderRow.Category)},
            p.description AS {nameof(ProviderRow.Description)},
            p.address AS {nameof(ProviderRow.Address)},
            ST_Y(p.location::geometry) AS {nameof(ProviderRow.Latitude)},
            ST_X(p.location::geometry) AS {nameof(ProviderRow.Longitude)},
            p.phone AS {nameof(ProviderRow.Phone)},
            p.claimed_at_utc IS NOT NULL AS {nameof(ProviderRow.IsClaimed)},
            p.booking_deposit_type AS {nameof(ProviderRow.DepositType)},
            p.booking_deposit_value AS {nameof(ProviderRow.DepositValue)},
            p.booking_deposit_due_days AS {nameof(ProviderRow.DepositDueDays)},
            p.booking_free_cancellation_days AS {nameof(ProviderRow.FreeCancellationDays)},
            p.venue_max_children AS {nameof(ProviderRow.MaxChildren)},
            p.venue_max_adults AS {nameof(ProviderRow.MaxAdults)},
            p.venue_min_age AS {nameof(ProviderRow.MinAge)},
            p.venue_max_age AS {nameof(ProviderRow.MaxAge)},
            p.venue_outside_catering AS {nameof(ProviderRow.OutsideCatering)},
            p.venue_outside_cake AS {nameof(ProviderRow.OutsideCake)},
            p.venue_food_service AS {nameof(ProviderRow.FoodService)},
            p.venue_has_own_animator AS {nameof(ProviderRow.HasOwnAnimator)},
            p.venue_has_own_photographer AS {nameof(ProviderRow.HasOwnPhotographer)},
            p.venue_has_smoking_area AS {nameof(ProviderRow.HasSmokingArea)}
        FROM providers p
        WHERE p.id = @ProviderId AND {ProviderSql.PubliclyVisible};

        SELECT c.id AS {nameof(PublicCityResponse.Id)}, c.slug AS {nameof(PublicCityResponse.Slug)},
               {LookupSql.Name} AS {nameof(PublicCityResponse.Name)},
               COALESCE({LookupSql.LocativeName}, {LookupSql.Name}) AS {nameof(PublicCityResponse.LocativeName)}
        FROM cities c
        JOIN providers p ON p.city_id = c.id
        WHERE p.id = @ProviderId;

        SELECT d.id AS {nameof(PublicDistrictResponse.Id)}, d.slug AS {nameof(PublicDistrictResponse.Slug)},
               {LookupSql.Name} AS {nameof(PublicDistrictResponse.Name)}
        FROM districts d
        JOIN providers p ON p.district_id = d.id
        WHERE p.id = @ProviderId;

        SELECT a.id AS {nameof(PublicAmenityResponse.Id)}, a.slug AS {nameof(PublicAmenityResponse.Slug)},
               {LookupSql.Name} AS {nameof(PublicAmenityResponse.Name)}
        FROM amenities a
        JOIN provider_amenities pa ON pa.amenity_id = a.id
        WHERE pa.provider_id = @ProviderId
        ORDER BY a.sort_order, a.slug;
        """;

    public async Task<Result<PublicProviderResponse>> Handle(GetPublicProviderQuery query, CancellationToken cancellationToken)
    {
        string slug = query.Slug.ToLowerInvariant();
        if (!UrlSlug.IsValid(slug))
        {
            return Result.Failure<PublicProviderResponse>(ProviderErrors.NotFound);
        }

        // Misses are removed right after, so a provider created or published later shows up at once
        string slugKey = ProviderCache.SlugKey(slug);
        Guid? providerId = await cacheService.GetOrCreateAsync(
            slugKey,
            token => LoadIdAsync(slug, token),
            ProviderCache.Expiration,
            cancellationToken: cancellationToken);

        if (providerId is not Guid id)
        {
            await cacheService.RemoveAsync(slugKey, cancellationToken);
            return Result.Failure<PublicProviderResponse>(ProviderErrors.NotFound);
        }

        string language = languageContext.Current.ToCode();
        string pageKey = ProviderCache.PageKey(id, language);
        PublicProviderResponse? page = await cacheService.GetOrCreateAsync(
            pageKey,
            token => LoadPageAsync(id, language, token),
            ProviderCache.Expiration,
            [ProviderCache.Tag(id), LookupsCache.Tag],
            cancellationToken);

        if (page is null)
        {
            await cacheService.RemoveAsync(pageKey, cancellationToken);
            return Result.Failure<PublicProviderResponse>(ProviderErrors.NotFound);
        }

        return page;
    }

    private async ValueTask<Guid?> LoadIdAsync(string slug, CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbConnectionFactory.OpenConnectionAsync();

        return await dbConnection.QuerySingleOrDefaultAsync<Guid?>(
            new CommandDefinition(IdSql, new { Slug = slug }, cancellationToken: cancellationToken));
    }

    private async ValueTask<PublicProviderResponse?> LoadPageAsync(Guid providerId, string language, CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbConnectionFactory.OpenConnectionAsync();

        await using SqlMapper.GridReader grid = await dbConnection.QueryMultipleAsync(
            new CommandDefinition(PageSql, new { ProviderId = providerId, Language = language }, cancellationToken: cancellationToken));

        ProviderRow? row = await grid.ReadSingleOrDefaultAsync<ProviderRow>();
        if (row is null)
        {
            return null;
        }

        PublicCityResponse city = await grid.ReadSingleAsync<PublicCityResponse>();
        PublicDistrictResponse? district = await grid.ReadSingleOrDefaultAsync<PublicDistrictResponse>();
        List<PublicAmenityResponse> amenities = [.. await grid.ReadAsync<PublicAmenityResponse>()];

        return new PublicProviderResponse
        {
            Id = row.Id,
            Slug = row.Slug,
            Name = row.Name,
            Category = Enum.Parse<ProviderCategory>(row.Category),
            Description = row.Description,
            Address = row.Address,
            City = city,
            District = district,
            Latitude = row.Latitude,
            Longitude = row.Longitude,
            Phone = row.Phone,
            IsClaimed = row.IsClaimed,
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
            Amenities = amenities
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
        public double Latitude { get; init; }
        public double Longitude { get; init; }
        public string? Phone { get; init; }
        public bool IsClaimed { get; init; }
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
    }
}
