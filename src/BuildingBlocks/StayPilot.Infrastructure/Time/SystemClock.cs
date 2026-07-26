using StayPilot.SharedKernel.Abstractions;

namespace StayPilot.Infrastructure.Time;

/// <summary>The real system clock. Registered as a singleton.</summary>
public sealed class SystemClock : IClock
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
