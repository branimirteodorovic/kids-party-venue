using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using KidsPartyVenue.Application.Lookups.GetCatalogs;
using KidsPartyVenue.Application.Lookups.GetCities;

namespace KidsPartyVenue.IntegrationTests.Lookups;

internal static class LookupRequests
{
    // The API writes enums as strings
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    public static async Task<CatalogsResponse> GetCatalogsAsync(this HttpClient client, string? acceptLanguage = null) =>
        (await client.GetLookupAsync<CatalogsResponse>("catalogs", acceptLanguage))!;

    public static async Task<List<CityResponse>> GetCitiesAsync(this HttpClient client, string? acceptLanguage = null) =>
        (await client.GetLookupAsync<List<CityResponse>>("cities", acceptLanguage))!;

    private static async Task<T?> GetLookupAsync<T>(this HttpClient client, string path, string? acceptLanguage)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, path);
        if (acceptLanguage is not null)
        {
            request.Headers.TryAddWithoutValidation("Accept-Language", acceptLanguage);
        }

        using HttpResponseMessage response = await client.SendAsync(request);
        response.IsSuccessStatusCode.Should().BeTrue();

        return await response.Content.ReadFromJsonAsync<T>(JsonOptions);
    }
}
