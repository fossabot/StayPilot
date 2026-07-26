using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace StayPilot.Application.Messaging;

/// <summary>
/// Default <see cref="IRequestDispatcher"/>. Resolves the handler and all
/// <see cref="IPipelineBehavior{TRequest,TResponse}"/> instances from DI and
/// composes them into a chain. Per-request-type wrappers are cached so the
/// reflection cost is paid once per closed request type.
/// </summary>
public sealed class RequestDispatcher(IServiceProvider serviceProvider) : IRequestDispatcher
{
    private static readonly ConcurrentDictionary<Type, RequestHandlerBase> Wrappers = new();

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var wrapper = (RequestHandlerWrapper<TResponse>)Wrappers.GetOrAdd(
            request.GetType(),
            requestType =>
            {
                var wrapperType = typeof(RequestHandlerWrapperImpl<,>)
                    .MakeGenericType(requestType, typeof(TResponse));
                return (RequestHandlerBase)Activator.CreateInstance(wrapperType)!;
            });

        return wrapper.Handle(request, serviceProvider, cancellationToken);
    }

    private abstract class RequestHandlerBase;

    private abstract class RequestHandlerWrapper<TResponse> : RequestHandlerBase
    {
        public abstract Task<TResponse> Handle(
            object request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken);
    }

    private sealed class RequestHandlerWrapperImpl<TRequest, TResponse> : RequestHandlerWrapper<TResponse>
        where TRequest : IRequest<TResponse>
    {
        public override Task<TResponse> Handle(
            object request,
            IServiceProvider serviceProvider,
            CancellationToken cancellationToken)
        {
            var typedRequest = (TRequest)request;

            Task<TResponse> Handler() =>
                serviceProvider
                    .GetRequiredService<IRequestHandler<TRequest, TResponse>>()
                    .Handle(typedRequest, cancellationToken);

            // Outermost-first registration; reverse so the first-registered
            // behavior ends up wrapping all the others.
            return serviceProvider
                .GetServices<IPipelineBehavior<TRequest, TResponse>>()
                .Reverse()
                .Aggregate(
                    (RequestHandlerDelegate<TResponse>)Handler,
                    (next, behavior) => () => behavior.Handle(typedRequest, next, cancellationToken))();
        }
    }
}
