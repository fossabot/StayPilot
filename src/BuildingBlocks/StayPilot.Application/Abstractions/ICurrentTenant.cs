namespace StayPilot.Application.Abstractions;

/// <summary>
/// The tenant (Organization) resolved for the current request. Populated by
/// tenant-resolution middleware and consumed by the persistence layer to apply
/// global query filters and stamp tenant-owned entities, audit rows, and
/// domain events. Application/handler code reads tenant context from here —
/// controllers never pass tenant identifiers around.
/// </summary>
public interface ICurrentTenant
{
    /// <summary>The resolved tenant id. Throws if accessed while unresolved.</summary>
    Guid OrganizationId { get; }

    /// <summary>True when a tenant has been resolved for this request.</summary>
    bool IsResolved { get; }

    /// <summary>Returns the tenant id if resolved; otherwise <c>null</c>.</summary>
    Guid? OrganizationIdOrNull { get; }
}
