namespace StayPilot.SharedKernel.Auditing;

/// <summary>
/// Marks an entity that is soft-deleted rather than physically removed.
/// A global query filter hides soft-deleted rows, and that filter is combined
/// with the tenant filter so soft-delete always respects tenant isolation.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; }

    DateTimeOffset? DeletedAtUtc { get; }

    string? DeletedBy { get; }
}
