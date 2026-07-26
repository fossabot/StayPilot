using Microsoft.Extensions.DependencyInjection;
using StayPilot.Application.Abstractions.Idempotency;
using StayPilot.Application.Messaging;
using StayPilot.Infrastructure.Behaviors;
using StayPilot.Infrastructure.Idempotency;
using StayPilot.Infrastructure.Messaging;
using StayPilot.Infrastructure.Persistence.Interceptors;
using StayPilot.Infrastructure.Time;
using StayPilot.SharedKernel.Abstractions;

namespace StayPilot.Infrastructure;

/// <summary>
/// Registers shared infrastructure: clock, domain-event dispatch, the save
/// interceptor, the idempotency store, and the two infrastructure-backed
/// pipeline behaviors. These behaviors are registered AFTER
/// <c>AddApplicationCore()</c>, giving the effective order
/// Logging → Performance → Validation → Authorization → Idempotency → Transaction → Handler.
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureShared(this IServiceCollection services)
    {
        services.AddSingleton<IClock, SystemClock>();
        services.AddScoped<IDomainEventDispatcher, DomainEventDispatcher>();
        services.AddScoped<AuditableTenantInterceptor>();

        // Placeholder store — swap for a Redis-backed implementation in production.
        services.AddSingleton<IIdempotencyStore, InMemoryIdempotencyStore>();

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(IdempotencyBehavior<,>));
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(TransactionBehavior<,>));

        return services;
    }
}
