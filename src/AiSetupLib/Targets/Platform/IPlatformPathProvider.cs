using AiSetup.Lib.Models;

namespace AiSetup.Lib.Targets.Platform;

/// <summary>
/// Returns the platform-specific local installation root for a deploy target.
/// </summary>
public interface IPlatformPathProvider
{
    /// <summary>Resolves the local root directory for <paramref name="target"/>.</summary>
    string GetLocalRoot(DeployTarget target);
}
