namespace StayPilot.SharedKernel.Domain;

/// <summary>
/// A fact that has happened within the domain. Every domain event carries the
/// owning <see cref="OrganizationId"/> (tenant) so that downstream handlers,
/// the outbox, and audit trails remain tenant-scoped end to end.
/// </summary>
public interface IDomainEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredOnUtc { get; }

    /// <summary>
    /// The owning tenant. Stamped by the aggregate when known, otherwise set by
    /// the persistence layer from <c>ICurrentTenant</c> before dispatch.
    /// </summary>
    Guid OrganizationId { get; }
}
