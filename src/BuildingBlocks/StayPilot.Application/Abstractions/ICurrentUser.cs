namespace StayPilot.Application.Abstractions;

/// <summary>The authenticated principal for the current request.</summary>
public interface ICurrentUser
{
    Guid? UserId { get; }

    string? UserName { get; }

    bool IsAuthenticated { get; }

    IReadOnlyCollection<string> Roles { get; }

    IReadOnlyCollection<string> Permissions { get; }

    bool HasPermission(string permission);

    bool IsInRole(string role);
}
