using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using AwesomeAssertions;
using KidsPartyVenue.Application.Providers;
using KidsPartyVenue.Application.Providers.CreateProvider;
using KidsPartyVenue.Application.Providers.GetProviderForEditing;
using KidsPartyVenue.Domain;
using KidsPartyVenue.Domain.Cities;
using KidsPartyVenue.Domain.Providers;
using KidsPartyVenue.Infrastructure.Identity;
using KidsPartyVenue.IntegrationTests.Abstractions;
using KidsPartyVenue.IntegrationTests.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.IntegrationTests.Providers;

/// <summary>Creating a provider to edit, and calling the editor's endpoints as the seeded admin.</summary>
public abstract class ProviderEditorTestBase(IntegrationTestWebAppFactory factory) : BaseIntegrationTest(factory)
{
    // Beograd has no districts in the seed
    protected const string CityWithoutDistrictsSlug = "beograd";

    // Inside Serbia, near the center of Novi Sad
    protected const double Latitude = 45.2389;
    protected const double Longitude = 19.8441;

    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IntegrationTestWebAppFactory _factory = factory;
    private string? _adminToken;

    /// <summary>A Draft venue in Novi Sad, in a district.</summary>
    protected async Task<Guid> CreateProviderAsync()
    {
        District district = await GetNoviSadDistrictAsync();
        Result<CreateProviderResponse> result = await SendCommand<CreateProviderCommand, CreateProviderResponse>(
            new CreateProviderCommand(
                UniqueName(), ProviderCategory.Venue, "Opis", Faker.Address.StreetAddress(), district.CityId, district.Id,
                Latitude, Longitude, "063 123 4567"));
        result.IsSuccess.Should().BeTrue();

        return result.Value.Id;
    }

    protected async Task<ProviderForEditingResponse> GetAsAdminAsync(Guid providerId)
    {
        HttpResponseMessage response = await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, await AdminTokenAsync());
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<ProviderForEditingResponse>(JsonOptions))!;
    }

    protected async Task<HttpResponseMessage> PutAsAdminAsync(Guid providerId, string section, object request) =>
        await SendAsync(HttpMethod.Put, $"providers/{providerId}/{section}", request, await AdminTokenAsync());

    protected async Task<HttpResponseMessage> GetRawAsAdminAsync(Guid providerId) =>
        await SendAsync(HttpMethod.Get, $"providers/{providerId}", null, await AdminTokenAsync());

    protected Task<HttpResponseMessage> SendAsync(HttpMethod method, string path, object? request, string? accessToken)
    {
        var message = new HttpRequestMessage(method, path);
        if (request is not null)
        {
            message.Content = JsonContent.Create(request, options: JsonOptions);
        }

        if (accessToken is not null)
        {
            message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
        }

        return HttpClient.SendAsync(message);
    }

    protected static async Task<uint> ReadVersionAsync(HttpResponseMessage response)
    {
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        return (await response.Content.ReadFromJsonAsync<ProviderVersionResponse>(JsonOptions))!.Version;
    }

    protected static async Task ShouldFailWith(HttpResponseMessage response, HttpStatusCode statusCode, string errorCode)
    {
        response.StatusCode.Should().Be(statusCode);
        ProblemDetails? problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        problem!.Title.Should().Be(errorCode);
    }

    protected async Task<District> GetNoviSadDistrictAsync()
    {
        Guid noviSadId = await GetCityIdAsync(DevelopmentSeeder.NoviSadSlug);

        return await DbContext.Districts.AsNoTracking().FirstAsync(d => d.CityId == noviSadId);
    }

    protected Task<Guid> GetCityIdAsync(string slug) =>
        DbContext.Cities.AsNoTracking().Where(c => c.Slug == slug).Select(c => c.Id).SingleAsync();

    protected Task<List<Guid>> GetAmenityIdsAsync(int count) =>
        DbContext.Amenities.AsNoTracking().OrderBy(a => a.SortOrder).Select(a => a.Id).Take(count).ToListAsync();

    protected static string UniqueName() => $"{Faker.Company.CompanyName()} {Guid.CreateVersion7():N}";

    protected async Task<string> AdminTokenAsync()
    {
        if (_adminToken is not null)
        {
            return _adminToken;
        }

        IConfiguration configuration = _factory.Services.GetRequiredService<IConfiguration>();
        HttpResponseMessage response = await HttpClient.LoginAsync(configuration["Seed:AdminEmail"]!, configuration["Seed:AdminPassword"]!);
        response.EnsureSuccessStatusCode();
        _adminToken = (await response.Content.ReadFromJsonAsync<LoginResponse>())!.AccessToken;

        return _adminToken;
    }

    protected async Task<string> ParentTokenAsync() => (await HttpClient.RegisterAndLoginAsync(Faker)).AccessToken;
}
