namespace StayPilot.Application.Behaviors;

/// <summary>
/// Opt-in marker for requests that require the current user to hold specific
/// permissions. The <see cref="AuthorizationBehavior{TRequest,TResponse}"/>
/// enforces these before the handler runs. Fine-grained, per-request checks
/// complement ASP.NET Core policy-based authorization at the endpoint.
/// </summary>
public interface IAuthorizedRequest
{
    /// <summary>Permissions the current user must all hold. Empty = authenticated only.</summary>
    IReadOnlyCollection<string> RequiredPermissions { get; }
}
