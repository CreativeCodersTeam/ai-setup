namespace AiSetup.Models;

/// <summary>
/// A predefined bundle of asset references (loaded from profiles/*.yaml).
/// </summary>
/// <param name="Name">Profile identifier (file stem).</param>
/// <param name="Description">Optional description.</param>
/// <param name="Agents">Agent references to include.</param>
/// <param name="Instructions">Instruction references to include.</param>
/// <param name="Skills">Skill references to include.</param>
/// <param name="McpConfigs">MCP config references to include.</param>
/// <param name="Settings">Target-specific settings fragment references to include.</param>
public sealed record Profile(
    string Name,
    string? Description,
    IReadOnlyList<ProfileAssetRef> Agents,
    IReadOnlyList<ProfileAssetRef> Instructions,
    IReadOnlyList<ProfileAssetRef> Skills,
    IReadOnlyList<ProfileAssetRef> McpConfigs,
    IReadOnlyList<ProfileAssetRef> Settings);
