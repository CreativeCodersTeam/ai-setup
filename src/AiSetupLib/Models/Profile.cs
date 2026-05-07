namespace AiSetup.Lib.Models;

/// <summary>
/// A pre-defined bundle of asset references to deploy together.
/// </summary>
/// <param name="Name">Profile identifier (matches the YAML file name).</param>
/// <param name="Description">Human-readable profile description.</param>
/// <param name="Agents">Agent asset names.</param>
/// <param name="Instructions">Instruction asset names (may include sub-folders).</param>
/// <param name="Skills">Skill asset names (may include sub-folders).</param>
/// <param name="McpConfigs">MCP config asset names.</param>
public sealed record Profile(
    string Name,
    string? Description,
    IReadOnlyList<string> Agents,
    IReadOnlyList<string> Instructions,
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> McpConfigs);
