using AiSetup.Models;

namespace AiSetup.Discovery;

/// <summary>
/// Read-only access to assets discovered in the source ai-setup repository.
/// </summary>
public interface IAssetRepository
{
    /// <summary>Looks up a single asset by type and ID.</summary>
    /// <param name="type">Asset type.</param>
    /// <param name="id">Asset identifier (relative path without extension).</param>
    /// <returns>The asset or null if not found.</returns>
    AssetDefinition? Find(AssetType type, string id);

    /// <summary>Returns all assets, optionally filtered by type.</summary>
    /// <param name="type">Optional filter.</param>
    /// <returns>Read-only list of assets.</returns>
    IReadOnlyList<AssetDefinition> All(AssetType? type = null);

    /// <summary>Returns warnings produced during discovery (e.g. malformed frontmatter).</summary>
    IReadOnlyList<string> Warnings { get; }
}
