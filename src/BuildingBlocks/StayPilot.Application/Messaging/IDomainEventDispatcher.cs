using StayPilot.SharedKernel.Domain;

namespace StayPilot.Application.Messaging;

/// <summary>
/// Dispatches domain events to their handlers. The persistence layer collects
/// events from tracked aggregates after a successful save and hands them here;
/// events are tenant-stamped before dispatch.
/// </summary>
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
