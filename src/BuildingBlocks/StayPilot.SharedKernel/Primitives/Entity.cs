namespace StayPilot.SharedKernel.Primitives;

/// <summary>
/// Base class for domain entities. Identity-based equality: two entities are
/// equal when they are of the same type and share the same <see cref="Id"/>.
/// </summary>
/// <typeparam name="TId">The strongly-typed identifier of the entity.</typeparam>
public abstract class Entity<TId> : IEquatable<Entity<TId>>
    where TId : notnull
{
    protected Entity(TId id) => Id = id;

    /// <summary>Parameterless constructor reserved for EF Core materialization.</summary>
    protected Entity()
    {
    }

    public TId Id { get; protected set; } = default!;

    public bool Equals(Entity<TId>? other) =>
        other is not null
        && GetType() == other.GetType()
        && EqualityComparer<TId>.Default.Equals(Id, other.Id);

    public override bool Equals(object? obj) => obj is Entity<TId> entity && Equals(entity);

    public override int GetHashCode() => EqualityComparer<TId>.Default.GetHashCode(Id);

    public static bool operator ==(Entity<TId>? left, Entity<TId>? right) => Equals(left, right);

    public static bool operator !=(Entity<TId>? left, Entity<TId>? right) => !Equals(left, right);
}
