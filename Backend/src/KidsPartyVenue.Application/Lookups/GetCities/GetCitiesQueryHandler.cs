using System.Data.Common;
using Dapper;
using KidsPartyVenue.Application.Abstractions.Localization;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Application.Messaging;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Localization;

namespace KidsPartyVenue.Application.Lookups.GetCities;

internal sealed class GetCitiesQueryHandler(
    IDbConnectionFactory dbConnectionFactory,
    ILanguageContext languageContext,
    ICacheService cacheService) : IQueryHandler<GetCitiesQuery, List<CityResponse>>
{
    private const string Sql = $"""
        SELECT id AS {nameof(CityResponse.Id)}, slug AS {nameof(CityResponse.Slug)}, name AS {nameof(CityResponse.Name)}
        FROM (SELECT id, slug, {LookupSql.Name} AS name FROM cities) all_cities
        {LookupSql.OrderByName};

        SELECT id AS {nameof(DistrictRow.Id)}, city_id AS {nameof(DistrictRow.CityId)}, slug AS {nameof(DistrictRow.Slug)},
               name AS {nameof(DistrictRow.Name)}
        FROM (SELECT id, city_id, slug, {LookupSql.Name} AS name FROM districts) all_districts
        {LookupSql.OrderByName};
        """;

    public async Task<Result<List<CityResponse>>> Handle(GetCitiesQuery query, CancellationToken cancellationToken)
    {
        Language language = languageContext.Current;

        return await cacheService.GetOrCreateAsync(
            $"cities:{language.ToCode()}",
            token => LoadAsync(language, token),
            LookupsCache.Expiration,
            [LookupsCache.Tag],
            cancellationToken);
    }

    private async ValueTask<List<CityResponse>> LoadAsync(Language language, CancellationToken cancellationToken)
    {
        await using DbConnection dbConnection = await dbConnectionFactory.OpenConnectionAsync();

        await using SqlMapper.GridReader grid = await dbConnection.QueryMultipleAsync(
            new CommandDefinition(Sql, new { Language = language.ToCode() }, cancellationToken: cancellationToken));

        List<CityResponse> cities = [.. await grid.ReadAsync<CityResponse>()];
        ILookup<Guid, DistrictRow> districts = (await grid.ReadAsync<DistrictRow>()).ToLookup(d => d.CityId);

        return [.. cities.Select(c => c with
        {
            Districts = [.. districts[c.Id].Select(d => new CityDistrictResponse { Id = d.Id, Slug = d.Slug, Name = d.Name })]
        })];
    }

    private sealed class DistrictRow
    {
        public Guid Id { get; init; }
        public Guid CityId { get; init; }
        public string Slug { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
    }
}
