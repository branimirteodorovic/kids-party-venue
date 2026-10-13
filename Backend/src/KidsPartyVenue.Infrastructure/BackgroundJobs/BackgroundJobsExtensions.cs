using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.Infrastructure.BackgroundJobs;

public static class BackgroundJobsExtensions
{
    internal static IServiceCollection AddBackgroundJobs(
        this IServiceCollection services,
        IConfiguration configuration,
        string databaseConnectionString)
    {
        services.AddHangfire(config => config
            .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
            .UseSimpleAssemblyNameTypeSerializer()
            .UseRecommendedSerializerSettings()
            .UsePostgreSqlStorage(
                options => options.UseNpgsqlConnection(databaseConnectionString),
                new PostgreSqlStorageOptions
                {
                    SchemaName = "hangfire",
                    PrepareSchemaIfNecessary = true
                }));

        if (configuration.GetValue("BackgroundJobs:ServerEnabled", true))
        {
            services.AddHangfireServer(options => options.WorkerCount = 5);
        }

        return services;
    }

    public static IApplicationBuilder UseBackgroundJobsDashboard(this IApplicationBuilder app, IConfiguration configuration)
    {
        if (configuration.GetValue("BackgroundJobs:DashboardEnabled", false))
        {
            app.UseHangfireDashboard("/jobs", new DashboardOptions
            {
                Authorization = [new DashboardAuthorizationFilter()],
                DashboardTitle = "KidsPartyVenue jobs"
            });
        }

        return app;
    }
}
