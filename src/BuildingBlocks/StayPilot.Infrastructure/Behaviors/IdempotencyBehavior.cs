using StayPilot.Application.Abstractions;
using StayPilot.Application.Abstractions.Idempotency;
using StayPilot.Application.Behaviors;
using StayPilot.Application.Messaging;
using StayPilot.SharedKernel.Results;

namespace StayPilot.Infrastructure.Behaviors;

/// <summary>
/// Ensures an <see cref="IIdempotentRequest"/> is processed at most once per
/// tenant + key. Duplicates short-circuit with a conflict; the key is recorded
/// only after the handler succeeds.
/// </summary>
public sealed class IdempotencyBehavior<TRequest, TResponse>(
    IIdempotencyStore idempotencyStore,
    ICurrentTenant currentTenant)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IIdempotentRequest
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var organizationId = currentTenant.OrganizationIdOrNull ?? Guid.Empty;

        if (await idempotencyStore.HasBeenProcessedAsync(organizationId, request.IdempotencyKey, cancellationToken))
        {
            return ResultFactory.Failure<TResponse>(
                Error.Conflict("idempotency.duplicate", "This request has already been processed."));
        }

        var response = await next();

        if (response.IsSuccess)
        {
            await idempotencyStore.MarkProcessedAsync(organizationId, request.IdempotencyKey, cancellationToken);
        }

        return response;
    }
}
