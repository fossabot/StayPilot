using Microsoft.Extensions.DependencyInjection;
using StayPilot.Application.Abstractions;
using StayPilot.Application.Messaging;
using StayPilot.SharedKernel.Domain;

namespace StayPilot.Infrastructure.Messaging;

/// <summary>
/// In-process domain event dispatcher. Tenant-stamps each event (if the raising
/// aggregate did not) and invokes every registered
/// <see cref="IDomainEventHandler{TEvent}"/>.
/// </summary>
/// <remarks>
/// In-process dispatch is the starting point. A transactional outbox should
/// replace this for at-least-once delivery across process restarts — the
/// interface stays the same, so handlers are unaffected.
/// </remarks>
public sealed class DomainEventDispatcher(
    IServiceProvider serviceProvider,
    ICurrentTenant currentTenant)
    : IDomainEventDispatcher
{
    public async Task DispatchAsync(
        IEnumerable<IDomainEvent> domainEvents,
        CancellationToken cancellationToken = default)
    {
        foreach (var domainEvent in domainEvents)
        {
            var stamped = Stamp(domainEvent);
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(stamped.GetType());
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.Handle))!;

            foreach (var handler in serviceProvider.GetServices(handlerType))
            {
                if (handler is null)
                {
                    continue;
                }

                await (Task)handleMethod.Invoke(handler, [stamped, cancellationToken])!;
            }
        }
    }

    private IDomainEvent Stamp(IDomainEvent domainEvent) =>
        domainEvent is DomainEvent record
        && record.OrganizationId == Guid.Empty
        && currentTenant.IsResolved
            ? record with { OrganizationId = currentTenant.OrganizationId }
            : domainEvent;
}
