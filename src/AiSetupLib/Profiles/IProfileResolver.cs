using AiSetup.Models;

namespace AiSetup.Profiles;

/// <summary>
/// Resolves a named profile into concrete asset definitions grouped by type.
/// </summary>
public interface IProfileResolver
{
    /// <summary>Resolves the profile named by <paramref name="options"/> into its assets.</summary>
    /// <param name="options">Deploy options containing the profile name.</param>
    /// <returns>Resolved assets grouped by type.</returns>
    ResolvedAssets Resolve(DeployOptions options);
}
