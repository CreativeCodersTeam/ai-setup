namespace AiSetup.Lib.Models;

/// <summary>
/// Aggregated user-supplied options for a deploy invocation.
/// </summary>
/// <param name="Target">Target AI system.</param>
/// <param name="Mode">Repo or local deploy mode.</param>
/// <param name="RepoPath">Target repository path (required when <see cref="Mode"/> is <see cref="DeployMode.Repo"/>).</param>
/// <param name="Profile">Optional profile name to expand into asset references.</param>
/// <param name="Agents">Explicit agent selection (in addition to profile content).</param>
/// <param name="Instructions">Explicit instruction selection.</param>
/// <param name="Skills">Explicit skill selection.</param>
/// <param name="McpConfigs">Explicit MCP-config selection.</param>
/// <param name="IncludeMcpConfigs">When false, MCP configs are skipped even if referenced by a profile.</param>
/// <param name="DryRun">When true, no files are written; the planned actions are only displayed.</param>
/// <param name="Force">When true, existing target files are overwritten without confirmation.</param>
public sealed record DeployOptions(
    DeployTarget Target,
    DeployMode Mode,
    string? RepoPath,
    string? Profile,
    IReadOnlyList<string> Agents,
    IReadOnlyList<string> Instructions,
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> McpConfigs,
    bool IncludeMcpConfigs,
    bool DryRun,
    bool Force);
