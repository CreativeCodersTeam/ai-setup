using AiSetup.Models;

namespace AiSetup.Platform;

/// <summary>
/// Resolves OS-specific destinations for local-mode deployments.
/// </summary>
public interface IPathProvider
{
    /// <summary>Returns the local-mode root directory for the given target.</summary>
    /// <param name="target">Target system.</param>
    /// <returns>Absolute directory path.</returns>
    string GetLocalRoot(DeployTarget target);
}
