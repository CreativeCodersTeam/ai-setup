namespace AiSetup.Models;

/// <summary>
/// Inputs for a deploy operation, produced by the CLI layer.
/// </summary>
public sealed class DeployOptions
{
    /// <summary>Target system to deploy to.</summary>
    public required DeployTarget Target { get; init; }

    /// <summary>Where the output is written.</summary>
    public required DeployMode Mode { get; init; }

    /// <summary>Source repository root (the ai-setup repo) to discover assets from.</summary>
    public required string SourceRepoPath { get; init; }

    /// <summary>Destination repository root, required when <see cref="Mode"/> is <see cref="DeployMode.Repo"/>.</summary>
    public string? DestinationRepoPath { get; init; }

    /// <summary>Optional profile name to expand into asset selections.</summary>
    public string? ProfileName { get; init; }

    /// <summary>Additional agent IDs (added on top of any profile selection).</summary>
    public IReadOnlyList<string> Agents { get; init; } = [];

    /// <summary>Additional instruction IDs.</summary>
    public IReadOnlyList<string> Instructions { get; init; } = [];

    /// <summary>Additional skill IDs.</summary>
    public IReadOnlyList<string> Skills { get; init; } = [];

    /// <summary>Additional MCP config IDs.</summary>
    public IReadOnlyList<string> McpConfigs { get; init; } = [];

    /// <summary>If true, only print the plan, do not write anything.</summary>
    public bool DryRun { get; init; }

    /// <summary>If true, overwrite existing files without prompting.</summary>
    public bool Force { get; init; }

    /// <summary>
    /// Strategy for handling MCP server name conflicts during the merge step.
    /// Independent of <see cref="Force"/>: if explicitly set, this takes precedence.
    /// </summary>
    public McpConflictResolution McpConflict { get; init; } = McpConflictResolution.Fail;
}
