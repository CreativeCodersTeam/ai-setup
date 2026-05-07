namespace AiSetup.Models;

/// <summary>
/// A predefined bundle of asset references (loaded from profiles/*.yaml).
/// </summary>
/// <param name="Name">Profile identifier (file stem).</param>
/// <param name="Description">Optional description.</param>
/// <param name="Agents">Agent IDs to include.</param>
/// <param name="Instructions">Instruction IDs to include.</param>
/// <param name="Skills">Skill IDs to include.</param>
/// <param name="McpConfigs">MCP config IDs to include.</param>
public sealed record Profile(
    string Name,
    string? Description,
    IReadOnlyList<string> Agents,
    IReadOnlyList<string> Instructions,
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> McpConfigs);
