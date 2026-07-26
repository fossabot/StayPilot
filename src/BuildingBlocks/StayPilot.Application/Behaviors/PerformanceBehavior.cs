using System.Diagnostics;
using Microsoft.Extensions.Logging;
using StayPilot.Application.Messaging;

namespace StayPilot.Application.Behaviors;

/// <summary>
/// Warns when a request exceeds <see cref="SlowThresholdMs"/>, surfacing slow
/// handlers early. Timing spans the inner pipeline (validation, handler, etc.).
/// </summary>
public sealed class PerformanceBehavior<TRequest, TResponse>(
    ILogger<PerformanceBehavior<TRequest, TResponse>> logger)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private const long SlowThresholdMs = 500;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var stopwatch = Stopwatch.StartNew();
        var response = await next();
        stopwatch.Stop();

        if (stopwatch.ElapsedMilliseconds > SlowThresholdMs)
        {
            logger.LogWarning(
                "Slow request {Request} took {ElapsedMilliseconds} ms (threshold {ThresholdMs} ms)",
                typeof(TRequest).Name,
                stopwatch.ElapsedMilliseconds,
                SlowThresholdMs);
        }

        return response;
    }
}
