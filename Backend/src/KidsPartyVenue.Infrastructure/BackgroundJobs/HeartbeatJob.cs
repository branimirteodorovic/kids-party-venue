using KidsPartyVenue.Application.Clock;
using Microsoft.Extensions.Logging;

namespace KidsPartyVenue.Infrastructure.BackgroundJobs;

internal sealed class HeartbeatJob(ILogger<HeartbeatJob> logger, IDateTimeProvider dateTimeProvider)
{
    public Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Heartbeat at {UtcNow}", dateTimeProvider.UtcNow);

        return Task.CompletedTask;
    }
}

