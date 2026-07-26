namespace StayPilot.Application.Behaviors;

/// <summary>
/// Opt-in marker: the request must run inside a single database transaction.
/// The transaction pipeline behavior lives in the infrastructure layer (it
/// needs the unit of work) and wraps the handler, committing on success and
/// rolling back on failure or exception.
/// </summary>
public interface ITransactionalRequest;
