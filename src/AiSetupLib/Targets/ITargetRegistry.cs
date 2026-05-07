using AiSetup.Models;

namespace AiSetup.Targets;

/// <summary>
/// Resolves an <see cref="IDeployTarget"/> by <see cref="DeployTarget"/> enum value.
/// </summary>
public interface ITargetRegistry
{
    /// <summary>Returns the deploy target adapter for <paramref name="target"/>.</summary>
    IDeployTarget Get(DeployTarget target);
}
