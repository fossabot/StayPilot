using StayPilot.Application.Abstractions;
using StayPilot.Application.Messaging;
using StayPilot.SharedKernel.Results;

namespace StayPilot.Application.Behaviors;

/// <summary>
/// Enforces <see cref="IAuthorizedRequest"/> requirements against the current
/// user. Only activates for requests that opt in via <see cref="IAuthorizedRequest"/>
/// and return a <see cref="Result"/>, so failures short-circuit as typed errors.
/// </summary>
public sealed class AuthorizationBehavior<TRequest, TResponse>(ICurrentUser currentUser)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>, IAuthorizedRequest
    where TResponse : Result
{
    public Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!currentUser.IsAuthenticated)
        {
            return Task.FromResult(
                ResultFactory.Failure<TResponse>(
                    Error.Unauthorized("auth.unauthenticated", "Authentication is required.")));
        }

        var missing = request.RequiredPermissions
            .Where(permission => !currentUser.HasPermission(permission))
            .ToList();

        if (missing.Count > 0)
        {
            return Task.FromResult(
                ResultFactory.Failure<TResponse>(
                    Error.Forbidden(
                        "auth.forbidden",
                        $"Missing required permission(s): {string.Join(", ", missing)}.")));
        }

        return next();
    }
}
