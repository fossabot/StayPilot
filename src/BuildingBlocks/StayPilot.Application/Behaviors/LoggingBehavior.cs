using Microsoft.Extensions.Logging;
using StayPilot.Application.Abstractions;
using StayPilot.Application.Messaging;

namespace StayPilot.Application.Behaviors;

/// <summary>
/// Outermost behavior: logs a scoped begin/end for every request, tagged with
/// the current tenant and user for correlation. Never swallows exceptions.
/// </summary>
public sealed class LoggingBehavior<TRequest, TResponse>(
    ILogger<LoggingBehavior<TRequest, TResponse>> logger,
    ICurrentTenant currentTenant,
    ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;

        using (logger.BeginScope(new Dictionary<string, object?>
        {
            ["Request"] = requestName,
            ["OrganizationId"] = currentTenant.OrganizationIdOrNull,
            ["UserId"] = currentUser.UserId,
        }))
        {
            logger.LogInformation("Handling {Request}", requestName);
            try
            {
                var response = await next();
                logger.LogInformation("Handled {Request}", requestName);
                return response;
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unhandled exception while handling {Request}", requestName);
                throw;
            }
        }
    }
}
