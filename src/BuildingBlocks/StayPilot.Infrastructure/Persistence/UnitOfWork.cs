using Microsoft.EntityFrameworkCore;
using StayPilot.Application.Abstractions.Persistence;

namespace StayPilot.Infrastructure.Persistence;

/// <summary>
/// <see cref="IUnitOfWork"/> over a specific module <typeparamref name="TContext"/>.
/// Uses the EF Core execution strategy so transactions are retry-safe on
/// transient PostgreSQL failures. Registered per module.
/// </summary>
public sealed class UnitOfWork<TContext>(TContext context) : IUnitOfWork
    where TContext : DbContext
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default) =>
        context.SaveChangesAsync(cancellationToken);

    public async Task ExecuteInTransactionAsync(
        Func<Task> operation,
        CancellationToken cancellationToken = default)
    {
        var strategy = context.Database.CreateExecutionStrategy();

        await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);
            await operation();
            await transaction.CommitAsync(cancellationToken);
        });
    }
}
