using StayPilot.SharedKernel.Results;

namespace StayPilot.Application.Messaging;

/// <summary>
/// Marker for a message dispatched through <see cref="IRequestDispatcher"/>.
/// <typeparamref name="TResponse"/> is the type the handler returns.
/// </summary>
public interface IRequest<TResponse>;

/// <summary>A state-changing operation that returns a value on success.</summary>
public interface ICommand<TResponse> : IRequest<Result<TResponse>>;

/// <summary>A state-changing operation with no return value beyond success/failure.</summary>
public interface ICommand : IRequest<Result>;

/// <summary>A read operation that returns data without changing state.</summary>
public interface IQuery<TResponse> : IRequest<Result<TResponse>>;
