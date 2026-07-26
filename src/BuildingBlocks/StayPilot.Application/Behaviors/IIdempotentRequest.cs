namespace StayPilot.Application.Behaviors;

/// <summary>
/// Opt-in marker: the request must be processed at most once per
/// <see cref="IdempotencyKey"/>. The idempotency pipeline behavior lives in the
/// infrastructure layer (it needs a durable/distributed store) and short-circuits
/// duplicate submissions.
/// </summary>
public interface IIdempotentRequest
{
    string IdempotencyKey { get; }
}
