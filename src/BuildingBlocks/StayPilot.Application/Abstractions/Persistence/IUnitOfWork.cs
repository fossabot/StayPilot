namespace StayPilot.Application.Abstractions.Persistence;

/// <summary>
/// Commits the current change set as one atomic unit. Implemented by the
/// persistence layer (EF Core). The transaction pipeline behavior uses this to
/// wrap <see cref="Behaviors.ITransactionalRequest"/> handlers.
/// </summary>
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

    Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default);
}
