namespace StayPilot.SharedKernel.Abstractions;

/// <summary>
/// Abstraction over the system clock so domain and application logic remain
/// deterministically testable. Infrastructure supplies the real implementation.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
