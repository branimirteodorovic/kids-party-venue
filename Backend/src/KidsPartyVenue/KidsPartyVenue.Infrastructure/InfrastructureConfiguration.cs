using Dapper;
using KidsPartyVenue.Application.Abstractions.Data;
using KidsPartyVenue.Application.Caching;
using KidsPartyVenue.Application.Clock;
using KidsPartyVenue.Application.Data;
using KidsPartyVenue.Infrastructure.Authentication;
using KidsPartyVenue.Infrastructure.Authorization;
using KidsPartyVenue.Infrastructure.Caching;
using KidsPartyVenue.Infrastructure.Clock;
using KidsPartyVenue.Infrastructure.Data;
using KidsPartyVenue.Infrastructure.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.EntityFrameworkCore.SqlServer.Extensions;
using StackExchange.Redis;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Migrations;

namespace KidsPartyVenue.Infrastructure;

public static class InfrastructureConfiguration
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string databaseConnectionString,
        string redisConnectionString)
    {
        services.AddAuthenticationInternal();

        services.AddAuthorizationInternal();

        services.TryAddSingleton<IDateTimeProvider, DateTimeProvider>();

        services.TryAddScoped<IDbConnectionFactory>(sp => new DbConnectionFactory(databaseConnectionString));

        services.AddDbContext<KidsPartyVenueDbContext>((sp, options) =>
            options
            .UseAzureSql(
                    databaseConnectionString,
                    sqlServerOptions => sqlServerOptions
                        .MigrationsHistoryTable(HistoryRepository.DefaultTableName))
                .UseSnakeCaseNamingConvention());

        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<KidsPartyVenueDbContext>());

        // TODO: Add repositories

        SqlMapper.AddTypeHandler(new GenericArrayHandler<string>());

        try
        {
            IConnectionMultiplexer connectionMultiplexer = ConnectionMultiplexer.Connect(redisConnectionString);
            services.AddSingleton(connectionMultiplexer);
            services.AddStackExchangeRedisCache(options =>
                options.ConnectionMultiplexerFactory = () => Task.FromResult(connectionMultiplexer));
        }
        catch
        {
            services.AddDistributedMemoryCache();
        }

        services.TryAddSingleton<ICacheService, CacheService>();

        return services;
    }
}
