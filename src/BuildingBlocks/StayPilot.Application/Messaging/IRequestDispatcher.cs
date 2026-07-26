namespace StayPilot.Application.Messaging;

/// <summary>
/// Sends a request through the pipeline (behaviors + handler). This is the
/// single seam the application depends on for CQRS dispatch — deliberately
/// small so the implementation can be swapped (e.g. for MediatR) later without
/// touching business logic.
/// </summary>
public interface IRequestDispatcher
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken cancellationToken = default);
}
