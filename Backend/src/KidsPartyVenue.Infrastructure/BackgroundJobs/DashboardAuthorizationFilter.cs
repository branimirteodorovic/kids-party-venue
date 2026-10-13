using Hangfire.Dashboard;

namespace KidsPartyVenue.Infrastructure.BackgroundJobs;

internal sealed class DashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context) => true;
}
