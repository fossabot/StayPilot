namespace StayPilot.Application.Messaging;

/// <summary>Continuation delegate that invokes the next behavior or the handler.</summary>
public delegate Task<TResponse> RequestHandlerDelegate<TResponse>();

/// <summary>
/// A cross-cutting step wrapped around request handling (validation, logging,
/// transactions, ...). Behaviors are chained in DI registration order, with the
/// first-registered behavior outermost.
/// </summary>
public interface IPipelineBehavior<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken);
}
