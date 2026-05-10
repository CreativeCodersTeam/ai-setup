namespace AiSetup.Models;

/// <summary>
/// Inputs for a deploy operation, produced by the CLI layer.
/// </summary>
public sealed class DeployOptions
{
    /// <summary>Target system to deploy to.</summary>
    public required DeployTarget Target { get; init; }

    /// <summary>Source repository root (the ai-setup repo) to discover assets from.</summary>
    public required string SourceRepoPath { get; init; }

    /// <summary>
    /// Destination repository root. Required when the resolved profile contains at least one
    /// asset deployed in <see cref="DeployMode.Repo"/> mode.
    /// </summary>
    public string? DestinationRepoPath { get; init; }

    /// <summary>Name of the profile to expand into the asset selection. Deployment is always profile-driven.</summary>
    public required string ProfileName { get; init; }

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
