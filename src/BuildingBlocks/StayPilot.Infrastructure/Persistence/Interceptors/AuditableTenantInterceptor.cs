using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using StayPilot.Application.Abstractions;
using StayPilot.SharedKernel.Abstractions;
using StayPilot.SharedKernel.Auditing;
using StayPilot.SharedKernel.Tenancy;

namespace StayPilot.Infrastructure.Persistence.Interceptors;

/// <summary>
/// On save, stamps tenant ownership, create/modify audit columns, and converts
/// hard deletes of <see cref="ISoftDeletable"/> entities into soft deletes.
/// All writes go through the EF Core property API so entities can keep these
/// members read-only to the domain.
/// </summary>
public sealed class AuditableTenantInterceptor(
    ICurrentTenant currentTenant,
    ICurrentUser currentUser,
    IClock clock)
    : SaveChangesInterceptor
{
    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        if (eventData.Context is not null)
        {
            Apply(eventData.Context);
        }

        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        if (eventData.Context is not null)
        {
            Apply(eventData.Context);
        }

        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void Apply(DbContext context)
    {
        var now = clock.UtcNow;
        var user = currentUser.UserId?.ToString();

        foreach (var entry in context.ChangeTracker.Entries())
        {
            if (entry.State is EntityState.Added && entry.Entity is ITenantOwned && currentTenant.IsResolved)
            {
                entry.Property(nameof(ITenantOwned.OrganizationId)).CurrentValue = currentTenant.OrganizationId;
            }

            if (entry.Entity is IAuditable)
            {
                switch (entry.State)
                {
                    case EntityState.Added:
                        entry.Property(nameof(IAuditable.CreatedAtUtc)).CurrentValue = now;
                        entry.Property(nameof(IAuditable.CreatedBy)).CurrentValue = user;
                        break;
                    case EntityState.Modified:
                        entry.Property(nameof(IAuditable.ModifiedAtUtc)).CurrentValue = now;
                        entry.Property(nameof(IAuditable.ModifiedBy)).CurrentValue = user;
                        break;
                }
            }

            if (entry.Entity is ISoftDeletable && entry.State is EntityState.Deleted)
            {
                entry.State = EntityState.Modified;
                entry.Property(nameof(ISoftDeletable.IsDeleted)).CurrentValue = true;
                entry.Property(nameof(ISoftDeletable.DeletedAtUtc)).CurrentValue = now;
                entry.Property(nameof(ISoftDeletable.DeletedBy)).CurrentValue = user;
            }
        }
    }
}
