using AiSetup.Lib.Models;

namespace AiSetup.Lib.Discovery;

/// <summary>
/// Discovers <see cref="AssetDefinition"/>s under a source repository root.
/// </summary>
public interface IAssetDiscovery
{
    /// <summary>
    /// Scans the well-known asset folders (<c>agents</c>, <c>instructions</c>,
    /// <c>skills</c>, <c>mcp-configs</c>) and returns all discovered assets.
    /// Files with invalid frontmatter are skipped and reported via the optional
    /// <paramref name="warningSink"/>.
    /// </summary>
    /// <param name="repoRoot">Source repository root.</param>
    /// <param name="warningSink">Optional collector for non-fatal warnings.</param>
    IReadOnlyList<AssetDefinition> Discover(string repoRoot, IList<string>? warningSink = null);
}
