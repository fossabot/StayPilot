using StayPilot.SharedKernel.Domain;

namespace StayPilot.SharedKernel.Primitives;

/// <summary>
/// Base class for aggregate roots — the only entities that may raise domain
/// events and the only valid entry points for persistence per aggregate.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier of the aggregate.</typeparam>
public abstract class AggregateRoot<TId> : Entity<TId>, IHasDomainEvents
    where TId : notnull
{
    private readonly List<IDomainEvent> _domainEvents = [];

    protected AggregateRoot(TId id) : base(id)
    {
    }

    protected AggregateRoot()
    {
    }

    /// <summary>Domain events raised by this aggregate since it was loaded/created.</summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void RaiseDomainEvent(IDomainEvent domainEvent) => _domainEvents.Add(domainEvent);

    /// <summary>Cleared by the persistence layer after events have been dispatched.</summary>
    public void ClearDomainEvents() => _domainEvents.Clear();
}
