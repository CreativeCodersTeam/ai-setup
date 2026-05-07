using AiSetup.Models;

namespace AiSetup.Aggregation;

/// <summary>
/// Merges YAML-defined MCP servers into the target system's JSON settings file.
/// </summary>
public interface IMcpConfigMerger
{
    /// <summary>
    /// Produces the merged JSON content.
    /// </summary>
    /// <param name="configs">MCP server assets (each parsed as a single server YAML).</param>
    /// <param name="serversKey">JSON property name to host the server map (see <see cref="McpServersKey"/>).</param>
    /// <param name="existingJson">Existing JSON content of the target settings file, or null if it does not exist.</param>
    /// <param name="overwriteOnConflict">When true, server entries in <paramref name="configs"/> overwrite existing ones.</param>
    /// <returns>UTF-8 JSON string with two-space indent.</returns>
    string Merge(
        IReadOnlyList<AssetDefinition> configs,
        string serversKey,
        string? existingJson,
        bool overwriteOnConflict);
}
