using System.Linq.Expressions;
using System.Reflection;
using Microsoft.EntityFrameworkCore;
using StayPilot.Application.Abstractions;
using StayPilot.Application.Messaging;
using StayPilot.Infrastructure.Persistence.QueryFilters;
using StayPilot.SharedKernel.Auditing;
using StayPilot.SharedKernel.Domain;
using StayPilot.SharedKernel.Tenancy;

namespace StayPilot.Infrastructure.Persistence;

/// <summary>
/// Base DbContext for every module. Applies tenant + soft-delete global query
/// filters automatically, and dispatches domain events after a successful save.
/// Module contexts inherit this and add their own <see cref="DbSet{TEntity}"/>s.
/// </summary>
public abstract class StayPilotDbContext : DbContext
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IDomainEventDispatcher _domainEventDispatcher;

    protected StayPilotDbContext(
        DbContextOptions options,
        ICurrentTenant currentTenant,
        IDomainEventDispatcher domainEventDispatcher)
        : base(options)
    {
        _currentTenant = currentTenant;
        _domainEventDispatcher = domainEventDispatcher;
    }

    /// <summary>
    /// Referenced by the tenant query filter and re-evaluated per query, so a
    /// single compiled model serves every tenant. <see cref="Guid.Empty"/> when
    /// unresolved (e.g. system/background context) — pair with
    /// <c>IgnoreQueryFilters()</c> for deliberate cross-tenant reads.
    /// </summary>
    protected Guid CurrentOrganizationId => _currentTenant.OrganizationIdOrNull ?? Guid.Empty;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            var clrType = entityType.ClrType;
            var isTenantOwned = typeof(ITenantOwned).IsAssignableFrom(clrType);
            var isSoftDeletable = typeof(ISoftDeletable).IsAssignableFrom(clrType);

            if (!isTenantOwned && !isSoftDeletable)
            {
                continue;
            }

            var filter = (LambdaExpression)BuildQueryFilterMethod
                .MakeGenericMethod(clrType)
                .Invoke(this, [isTenantOwned, isSoftDeletable])!;

            modelBuilder.Entity(clrType).HasQueryFilter(filter);

            // Tenant-owned entities are always filtered/joined by OrganizationId —
            // index it for performance.
            if (isTenantOwned)
            {
                modelBuilder.Entity(clrType).HasIndex(nameof(ITenantOwned.OrganizationId));
            }
        }
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Collect BEFORE save so entities still have their events; dispatch AFTER
        // a successful save so handlers observe committed state.
        var domainEvents = ChangeTracker
            .Entries<IHasDomainEvents>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        var result = await base.SaveChangesAsync(cancellationToken);

        foreach (var entry in ChangeTracker.Entries<IHasDomainEvents>())
        {
            entry.Entity.ClearDomainEvents();
        }

        if (domainEvents.Count > 0)
        {
            await _domainEventDispatcher.DispatchAsync(domainEvents, cancellationToken);
        }

        return result;
    }

    private static readonly MethodInfo BuildQueryFilterMethod = typeof(StayPilotDbContext)
        .GetMethod(nameof(BuildQueryFilter), BindingFlags.Instance | BindingFlags.NonPublic)!;

    private LambdaExpression BuildQueryFilter<TEntity>(bool isTenantOwned, bool isSoftDeletable)
        where TEntity : class
    {
        Expression<Func<TEntity, bool>>? filter = null;

        if (isTenantOwned)
        {
            filter = entity => EF.Property<Guid>(entity, nameof(ITenantOwned.OrganizationId)) == CurrentOrganizationId;
        }

        if (isSoftDeletable)
        {
            Expression<Func<TEntity, bool>> notDeleted =
                entity => !EF.Property<bool>(entity, nameof(ISoftDeletable.IsDeleted));

            filter = filter is null ? notDeleted : filter.AndAlso(notDeleted);
        }

        return filter!;
    }
}
