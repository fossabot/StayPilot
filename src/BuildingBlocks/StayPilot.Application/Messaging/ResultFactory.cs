using System.Collections.Concurrent;
using System.Reflection;
using StayPilot.SharedKernel.Results;

namespace StayPilot.Application.Messaging;

/// <summary>
/// Builds a failed <see cref="Result"/> / <see cref="Result{T}"/> for a generic
/// <c>TResponse</c> so behaviors (validation, authorization, idempotency) can
/// short-circuit the pipeline with a typed failure instead of throwing. Public
/// so behaviors in the infrastructure layer can reuse it.
/// </summary>
public static class ResultFactory
{
    private static readonly ConcurrentDictionary<Type, MethodInfo> GenericFailureCache = new();

    private static readonly MethodInfo GenericFailureDefinition = typeof(Result)
        .GetMethods(BindingFlags.Public | BindingFlags.Static)
        .First(m => m is { Name: nameof(Result.Failure), IsGenericMethod: true });

    public static TResponse Failure<TResponse>(Error error)
        where TResponse : Result
    {
        var responseType = typeof(TResponse);

        if (responseType == typeof(Result))
        {
            return (TResponse)Result.Failure(error);
        }

        // Result<TValue> — invoke Result.Failure<TValue>(error) via cached MethodInfo.
        var closed = GenericFailureCache.GetOrAdd(
            responseType.GetGenericArguments()[0],
            valueType => GenericFailureDefinition.MakeGenericMethod(valueType));

        return (TResponse)closed.Invoke(null, [error])!;
    }
}
