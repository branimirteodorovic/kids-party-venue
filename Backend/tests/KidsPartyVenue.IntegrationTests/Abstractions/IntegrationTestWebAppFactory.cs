using System.Security.Cryptography;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Testcontainers.PostgreSql;

namespace KidsPartyVenue.IntegrationTests.Abstractions;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>, IAsyncLifetime
{
    private readonly PostgreSqlContainer _dbContainer = new PostgreSqlBuilder("postgis/postgis:18-3.6")
        .WithDatabase("kids-party-venue")
        .WithUsername("postgres")
        .WithPassword("postgres")
        .Build();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("ConnectionStrings__Database", _dbContainer.GetConnectionString());
        Environment.SetEnvironmentVariable("BackgroundJobs__ServerEnabled", "false");
        // CI has no user secrets; a random key per run is enough for tests
        Environment.SetEnvironmentVariable(
            "Authentication__SigningKey",
            Convert.ToBase64String(RandomNumberGenerator.GetBytes(48)));
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }
}
