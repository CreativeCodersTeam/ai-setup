using AiSetup.Lib.Models;

namespace AiSetup.Lib.Aggregation;

/// <summary>
/// Builds JSON content for MCP server configurations from YAML asset bodies,
/// merging with an optional existing JSON document.
/// </summary>
public interface IMcpConfigMerger
{
    /// <summary>
    /// Produces a JSON document containing the merged set of MCP server entries.
    /// </summary>
    /// <param name="assets">MCP-config assets to be deployed.</param>
    /// <param name="existingJson">Existing JSON content at the target (or null when none exists).</param>
    /// <param name="settingsRootKey">
    /// JSON property under which MCP server entries live in the target file
    /// (e.g. <c>mcpServers</c> for Claude or <c>servers</c> for VS Code).
    /// When null, the merged content is the entries object itself.
    /// </param>
    string Merge(IEnumerable<AssetDefinition> assets, string? existingJson, string? settingsRootKey);
}
