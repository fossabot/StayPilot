using FluentValidation;
using StayPilot.Application.Messaging;
using StayPilot.SharedKernel.Results;

namespace StayPilot.Application.Behaviors;

/// <summary>
/// Runs all FluentValidation validators registered for the request. On failure
/// it short-circuits the pipeline with a typed <see cref="Result"/> failure
/// (no exception). Only applies to requests whose response is a
/// <see cref="Result"/>, guaranteeing a typed failure can be produced.
/// </summary>
public sealed class ValidationBehavior<TRequest, TResponse>(IEnumerable<IValidator<TRequest>> validators)
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (!validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var results = await Task.WhenAll(
            validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = results
            .SelectMany(r => r.Errors)
            .Where(f => f is not null)
            .ToList();

        if (failures.Count == 0)
        {
            return await next();
        }

        var message = string.Join("; ", failures.Select(f => $"{f.PropertyName}: {f.ErrorMessage}"));
        return ResultFactory.Failure<TResponse>(Error.Validation("validation.failed", message));
    }
}
