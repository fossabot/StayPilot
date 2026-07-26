namespace StayPilot.SharedKernel.Domain;

/// <summary>
/// Non-generic view over an aggregate's domain events, so the persistence layer
/// can collect and clear events without knowing the aggregate's id type.
/// </summary>
public interface IHasDomainEvents
{
    IReadOnlyCollection<IDomainEvent> DomainEvents { get; }

    void ClearDomainEvents();
}
