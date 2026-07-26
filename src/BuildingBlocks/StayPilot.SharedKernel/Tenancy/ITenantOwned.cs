namespace StayPilot.SharedKernel.Tenancy;

/// <summary>
/// Marks an entity as owned by a tenant (Organization). Every tenant-owned
/// entity carries an <see cref="OrganizationId"/>; global reference entities
/// (Country, Currency, TimeZone, AmenityCatalog, ...) must NOT implement this.
/// </summary>
/// <remarks>
/// The property is read-only to the domain: the value is assigned by the
/// persistence layer on insert (from <c>ICurrentTenant</c>) via the EF Core
/// property API, and enforced on read by a global query filter. Controllers
/// never see or set tenant filtering.
/// </remarks>
public interface ITenantOwned
{
    Guid OrganizationId { get; }
}
