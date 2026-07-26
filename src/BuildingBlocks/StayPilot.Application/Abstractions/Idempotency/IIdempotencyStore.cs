namespace StayPilot.Application.Abstractions.Idempotency;

/// <summary>
/// Durable/distributed store recording processed idempotency keys, scoped per
/// tenant. Backed by Redis or the database in the infrastructure layer.
/// </summary>
public interface IIdempotencyStore
{
    /// <summary>Returns true if this key has already been processed for the tenant.</summary>
    Task<bool> HasBeenProcessedAsync(Guid organizationId, string key, CancellationToken cancellationToken = default);

    /// <summary>Records the key as processed for the tenant.</summary>
    Task MarkProcessedAsync(Guid organizationId, string key, CancellationToken cancellationToken = default);
}
