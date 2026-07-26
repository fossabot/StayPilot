using StayPilot.SharedKernel.Domain;

namespace StayPilot.Application.Messaging;

/// <summary>
/// Handles a domain event. Multiple handlers may exist per event; all are
/// invoked. Event-driven side effects live here, decoupled from the aggregate.
/// </summary>
public interface IDomainEventHandler<in TEvent>
    where TEvent : IDomainEvent
{
    Task Handle(TEvent domainEvent, CancellationToken cancellationToken);
}
