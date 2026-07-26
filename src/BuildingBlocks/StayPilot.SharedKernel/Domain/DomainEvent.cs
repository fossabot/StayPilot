namespace StayPilot.SharedKernel.Domain;

/// <summary>
/// Immutable base record for domain events. Records enable the persistence
/// layer to tenant-stamp an event non-destructively via
/// <c>evt with { OrganizationId = tenantId }</c> when the raising aggregate
/// did not already supply it.
/// </summary>
public abstract record DomainEvent : IDomainEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredOnUtc { get; init; } = DateTimeOffset.UtcNow;

    public Guid OrganizationId { get; init; }
}
