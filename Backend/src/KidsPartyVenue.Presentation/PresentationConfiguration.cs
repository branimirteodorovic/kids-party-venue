using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.Presentation;

public static class PresentationConfiguration
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services)
    {
        services.AddEndpoints();

        return services;
    }
}
