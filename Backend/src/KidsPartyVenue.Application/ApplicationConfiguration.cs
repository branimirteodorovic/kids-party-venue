using System.Reflection;
using FluentValidation;
using KidsPartyVenue.Application.Behaviors;
using KidsPartyVenue.Application.Messaging;
using Microsoft.Extensions.DependencyInjection;

namespace KidsPartyVenue.Application;

public static class ApplicationConfiguration
{
    public static IServiceCollection AddApplication(
        this IServiceCollection services,
        Assembly assembly)
    {
        services.Scan(scan => scan.FromAssemblies(assembly)
            .AddClasses(classes => classes.AssignableTo(typeof(IQueryHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime()
            .AddClasses(classes => classes.AssignableTo(typeof(ICommandHandler<,>)), publicOnly: false)
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        // Decorators run in reverse order of registration
        // Register: Validation -> Logging -> ExceptionHandling
        // Runtime: ExceptionHandling -> Logging -> Validation

        services.TryDecorate(typeof(ICommandHandler<,>), typeof(ValidationDecorator.CommandHandlerT<,>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(ValidationDecorator.CommandHandler<>));

        services.TryDecorate(typeof(ICommandHandler<,>), typeof(LoggingDecorator.CommandHandlerT<,>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(LoggingDecorator.CommandHandler<>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(LoggingDecorator.QueryHandler<,>));

        services.TryDecorate(typeof(ICommandHandler<,>), typeof(ExceptionHandlingDecorator.CommandHandlerT<,>));
        services.TryDecorate(typeof(ICommandHandler<>), typeof(ExceptionHandlingDecorator.CommandHandler<>));
        services.TryDecorate(typeof(IQueryHandler<,>), typeof(ExceptionHandlingDecorator.QueryHandler<,>));

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
