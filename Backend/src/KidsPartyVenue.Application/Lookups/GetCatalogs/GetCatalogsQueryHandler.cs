using System.Data.Common;
using Dapper;
using KidsPartyVenue.Application.Abstractions.Localization;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Localization;
using KidsPartyVenue.Domain.Providers;

namespace KidsPartyVenue.Application.Lookups.GetCatalogs;

internal sealed class GetCatalogsQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ILanguageContext languageContext,
    ICacheService cacheService) : IQueryHandler<GetCatalogsQuery, CatalogsResponse>
{
    // A city is open when at least one category is open in it
    private const string Sql = $"""
        SELECT id AS {nameof(CityRow.Id)}, slug AS {nameof(CityRow.Slug)}, name AS {nameof(CityRow.Name)},
               locative_name AS {nameof(CityRow.LocativeName)}, latitude AS {nameof(CityRow.Latitude)},
               longitude AS {nameof(CityRow.Longitude)}
        FROM (
            SELECT c.id, c.slug, {LookupSql.Name} AS name,
                   COALESCE({LookupSql.LocativeName}, {LookupSql.Name}) AS locative_name,
                   ST_Y(c.center::geometry) AS latitude, ST_X(c.center::geometry) AS longitude
            FROM cities c
            WHERE EXISTS (SELECT 1 FROM city_categories cc WHERE cc.city_id = c.id)
        ) open_cities
        {LookupSql.OrderByName};

        SELECT city_id AS {nameof(CategoryRow.CityId)}, category AS {nameof(CategoryRow.Category)}
        FROM city_categories
        ORDER BY category;

        SELECT id AS {nameof(DistrictRow.Id)}, city_id AS {nameof(DistrictRow.CityId)}, slug AS {nameof(DistrictRow.Slug)},
               name AS {nameof(DistrictRow.Name)}
        FROM (
            SELECT d.id, d.city_id, d.slug, {LookupSql.Name} AS name
            FROM districts d
            WHERE EXISTS (SELECT 1 FROM city_categories cc WHERE cc.city_id = d.city_id)
        ) open_districts
        {LookupSql.OrderByName};

        SELECT id AS {nameof(CatalogAmenityResponse.Id)}, slug AS {nameof(CatalogAmenityResponse.Slug)},
               {LookupSql.Name} AS {nameof(CatalogAmenityResponse.Name)}
        FROM amenities
        ORDER BY sort_order, slug;
        """;

    public async Task<Result<CatalogsResponse>> Handle(GetCatalogsQuery query, CancellationToken cancellationToken)
    {
        Language language = languageContext.Current;

        return await cacheService.GetOrCreateAsync(
            $"catalogs:{language.ToCode()}",
            token => LoadAsync(language, token),
            LookupsCache.Expiration,
            [LookupsCache.Tag],
            cancellationToken);
    }

    private async ValueTask<CatalogsResponse> LoadAsync(Language language, CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbConnectionFactory.OpenConnectionAsync();

        await using SqlMapper.GridReader grid = await dbConnection.QueryMultipleAsync(
            new CommandDefinition(Sql, new { Language = language.ToCode() }, cancellationToken: cancellationToken));

        List<CityRow> cities = [.. await grid.ReadAsync<CityRow>()];
        ILookup<Guid, ProviderCategory> categories = (await grid.ReadAsync<CategoryRow>())
            .ToLookup(c => c.CityId, c => Enum.Parse<ProviderCategory>(c.Category));
        ILookup<Guid, DistrictRow> districts = (await grid.ReadAsync<DistrictRow>()).ToLookup(d => d.CityId);
        List<CatalogAmenityResponse> amenities = [.. await grid.ReadAsync<CatalogAmenityResponse>()];

        return new CatalogsResponse
        {
            Cities = [.. cities.Select(c => new CatalogCityResponse
            {
                Id = c.Id,
                Slug = c.Slug,
                Name = c.Name,
                LocativeName = c.LocativeName,
                Center = new CoordinateResponse { Latitude = c.Latitude, Longitude = c.Longitude },
                Categories = [.. categories[c.Id]],
                Districts = [.. districts[c.Id].Select(d => new CatalogDistrictResponse { Id = d.Id, Slug = d.Slug, Name = d.Name })]
            })],
            Amenities = amenities
        };
    }

    private sealed class CityRow
    {
        public Guid Id { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string LocativeName { get; init; } = string.Empty;
        public double Latitude { get; init; }
        public double Longitude { get; init; }
    }

    private sealed class CategoryRow
    {
        public Guid CityId { get; init; }
        public string Category { get; init; } = string.Empty;
    }

    private sealed class DistrictRow
    {
        public Guid Id { get; init; }
        public Guid CityId { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}
