using KidsPartyVenue.Presentation.Endpoints;
using Microsoft.Extensions.DependencyInjection;
using System.Text.Json.Serialization;

namespace KidsPartyVenue.Presentation;

public static class PresentationConfiguration
{
    public static IServiceCollection AddPresentation(
        this IServiceCollection services)
    {
        services.AddEndpoints();

        services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter()));

        return services;
    }
}
