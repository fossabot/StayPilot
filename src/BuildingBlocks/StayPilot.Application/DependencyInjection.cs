using System.Reflection;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using StayPilot.Application.Behaviors;
using StayPilot.Application.Messaging;

namespace StayPilot.Application;

/// <summary>
/// Registers the CQRS core: the dispatcher and the cross-cutting pipeline
/// behaviors. Behaviors are registered outermost-first — the infrastructure
/// layer appends the idempotency and transaction behaviors after these, so the
/// effective order is:
/// Logging → Performance → Validation → Authorization → Idempotency → Transaction → Handler.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApplicationCore(this IServiceCollection services)
    {
        services.AddScoped<IRequestDispatcher, RequestDispatcher>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(LoggingBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(PerformanceBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehavior<,>));

        return services;
    }

    /// <summary>
    /// Scans a module assembly for request handlers and FluentValidation
    /// validators and registers them. Called once per module during startup.
    /// </summary>
    public static IServiceCollection AddModuleHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterface = typeof(IRequestHandler<,>);

        var implementations = assembly
            .GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false, IsGenericTypeDefinition: false });

        foreach (var implementation in implementations)
        {
            foreach (var service in implementation.GetInterfaces()
                         .Where(i => i.IsGenericType && i.GetGenericTypeDefinition() == handlerInterface))
            {
                services.AddTransient(service, implementation);
            }
        }

        services.AddValidatorsFromAssembly(assembly, includeInternalTypes: true);

        return services;
    }
}
