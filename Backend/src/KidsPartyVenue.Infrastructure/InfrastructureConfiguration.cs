using Dapper;
using KidsPartyVenue.Application.Abstractions.Authentication;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Infrastructure.Authentication;
using KidsPartyVenue.Infrastructure.Authorization;
using KidsPartyVenue.Infrastructure.BackgroundJobs;
using KidsPartyVenue.Infrastructure.Caching;
using KidsPartyVenue.Infrastructure.Clock;
using KidsPartyVenue.Infrastructure.Data;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Npgsql;

namespace KidsPartyVenue.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration,
        string databaseConnectionString)
    {
        services.AddAuthenticationInternal();

        services.AddAuthorizationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        // One data source (connection pool) shared by Dapper, EF Core and the health check
        NpgsqlDataSource npgsqlDataSource = new NpgsqlDataSourceBuilder(databaseConnectionString)
            .UseNetTopologySuite()
            .Build();
        services.TryAddSingleton(npgsqlDataSource);
        services.TryAddScoped<IDbConnectionFactory, DbConnectionFactory>();

        services.AddDbContext<KidsPartyVenueDbContext>(options =>
            options
                .UseNpgsql(
                    npgsqlDataSource,
                    npgsqlOptions => npgsqlOptions.UseNetTopologySuite())
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<KidsPartyVenueDbContext>());

        // TODO: Add repositories

        SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());

        services.AddHybridCache(o => o.DefaultEntryOptions = new() 
        {
            Expiration = TimeSpan.FromMinutes(configuration.GetValue("Caching:DefaultExpirationInMinutes", 5)) 
        });
        services.TryAddSingleton<ICacheService, CacheService>();

        services.AddBackgroundJobs(configuration, databaseConnectionString);

        return services;
    }
}
