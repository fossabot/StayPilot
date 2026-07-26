using StayPilot.SharedKernel.Results;

namespace StayPilot.Application.Messaging;

/// <summary>Handles a <see cref="IRequest{TResponse}"/>. Resolved from DI by the dispatcher.</summary>
public interface IRequestHandler<in TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken);
}

/// <summary>Handles an <see cref="ICommand{TResponse}"/>.</summary>
public interface ICommandHandler<in TCommand, TResponse> : IRequestHandler<TCommand, Result<TResponse>>
    where TCommand : ICommand<TResponse>;

/// <summary>Handles an <see cref="ICommand"/>.</summary>
public interface ICommandHandler<in TCommand> : IRequestHandler<TCommand, Result>
    where TCommand : ICommand;

/// <summary>Handles an <see cref="IQuery{TResponse}"/>.</summary>
public interface IQueryHandler<in TQuery, TResponse> : IRequestHandler<TQuery, Result<TResponse>>
    where TQuery : IQuery<TResponse>;
