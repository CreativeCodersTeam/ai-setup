using AiSetup.Models;

namespace AiSetup.Profiles;

/// <summary>
/// Resolves a profile and CLI overrides into a flat list of concrete asset definitions.
/// </summary>
public interface IProfileResolver
{
    /// <summary>Resolves the asset selection described by <paramref name="options"/>.</summary>
    /// <param name="options">Deploy options containing profile and explicit selections.</param>
    /// <returns>Resolved assets grouped by type.</returns>
    ResolvedAssets Resolve(DeployOptions options);
}
