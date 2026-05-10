namespace AiSetup.Profiles;

/// <summary>
/// Result of resolving a profile into concrete assets, each tagged with its deploy mode.
/// </summary>
/// <param name="Agents">Resolved agent assets.</param>
/// <param name="Instructions">Resolved instruction assets.</param>
/// <param name="Skills">Resolved skill assets.</param>
/// <param name="McpConfigs">Resolved MCP configuration assets.</param>
public sealed record ResolvedAssets(
    IReadOnlyList<ResolvedAsset> Agents,
    IReadOnlyList<ResolvedAsset> Instructions,
    IReadOnlyList<ResolvedAsset> Skills,
    IReadOnlyList<ResolvedAsset> McpConfigs)
{
    /// <summary>Empty resolution result.</summary>
    public static readonly ResolvedAssets Empty = new([], [], [], []);

    /// <summary>Returns all selected assets in a single sequence.</summary>
    public IEnumerable<ResolvedAsset> Combined()
    {
        foreach (var asset in Instructions)
        {
            yield return asset;
        }

        foreach (var asset in Agents)
        {
            yield return asset;
        }

        foreach (var asset in Skills)
        {
            yield return asset;
        }

        foreach (var asset in McpConfigs)
        {
            yield return asset;
        }
    }
}
