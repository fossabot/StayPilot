using StayPilot.Application.Abstractions.Persistence;
using StayPilot.Application.Behaviors;
using StayPilot.Application.Messaging;

namespace StayPilot.Infrastructure.Behaviors;

/// <summary>
/// Wraps handlers of <see cref="ITransactionalRequest"/> in a single database
/// transaction: commit on success, roll back on exception. Only activates for
/// opted-in requests (generic constraint), so read queries pay no cost.
/// </summary>
public sealed class TransactionBehavior<TRequest, TResponse>(IUnitOfWork unitOfWork)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, ITransactionalRequest
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        TResponse response = default!;

        await unitOfWork.ExecuteInTransactionAsync(
            async () => response = await next(),
            cancellationToken);

        return response;
    }
}
