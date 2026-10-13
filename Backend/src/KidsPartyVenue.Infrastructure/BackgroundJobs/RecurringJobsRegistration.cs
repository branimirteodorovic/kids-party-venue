using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.Infrastructure.BackgroundJobs;

public static class RecurringJobsRegistration
{
    private static readonly TimeZoneInfo Belgrade = TimeZoneInfo.FindSystemTimeZoneById("Europe/Belgrade");

    public static void RegisterRecurringJobs(this IApplicationBuilder app)
    {
        IRecurringJobManager jobs = app.ApplicationServices.GetRequiredService<IRecurringJobManager>();
        var options = new RecurringJobOptions { TimeZone = Belgrade };

        jobs.AddOrUpdate<HeartbeatJob>(
            "heartbeat",
            job => job.ExecuteAsync(CancellationToken.None),
            Cron.Minutely(),
            options);

        // Later: "slots-generate" daily 02:00, "outbox-process" every 15s (see M2), ...
    }
}

