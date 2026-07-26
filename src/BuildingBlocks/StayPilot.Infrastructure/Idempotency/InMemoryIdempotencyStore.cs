using System.Collections.Concurrent;
using StayPilot.Application.Abstractions.Idempotency;

namespace StayPilot.Infrastructure.Idempotency;

/// <summary>
/// Placeholder in-memory idempotency store — process-local and non-durable.
/// Suitable for development and tests only. Replace with a Redis-backed store
/// (with TTL) for production so idempotency holds across instances and restarts.
/// </summary>
public sealed class InMemoryIdempotencyStore : IIdempotencyStore
{
    private readonly ConcurrentDictionary<string, byte> _processed = new();

    public Task<bool> HasBeenProcessedAsync(Guid organizationId, string key, CancellationToken cancellationToken = default) =>
        Task.FromResult(_processed.ContainsKey(Compose(organizationId, key)));

    public Task MarkProcessedAsync(Guid organizationId, string key, CancellationToken cancellationToken = default)
    {
        _processed.TryAdd(Compose(organizationId, key), 0);
        return Task.CompletedTask;
    }

    private static string Compose(Guid organizationId, string key) => $"{organizationId:N}:{key}";
}
