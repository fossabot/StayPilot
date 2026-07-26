namespace StayPilot.SharedKernel.Auditing;

/// <summary>
/// Marks an entity whose create/modify audit columns are stamped automatically
/// by the persistence layer (via the audit interceptor). Values are read-only
/// to the domain and written through the EF Core property API.
/// </summary>
public interface IAuditable
{
    DateTimeOffset CreatedAtUtc { get; }

    string? CreatedBy { get; }

    DateTimeOffset? ModifiedAtUtc { get; }

    string? ModifiedBy { get; }
}
