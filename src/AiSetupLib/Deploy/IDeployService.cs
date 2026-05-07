using AiSetup.Lib.Models;

namespace AiSetup.Lib.Deploy;

/// <summary>
/// Result of a deploy invocation.
/// </summary>
/// <param name="Plan">Planned actions (always populated).</param>
/// <param name="Warnings">Non-fatal warnings collected during planning.</param>
/// <param name="Applied">True when the plan was actually executed (i.e. not a dry-run).</param>
public sealed record DeployResult(DeployPlan Plan, IReadOnlyList<string> Warnings, bool Applied);

/// <summary>
/// Orchestrates profile resolution, asset discovery and target deployment.
/// </summary>
public interface IDeployService
{
    /// <summary>
    /// Discovers assets, resolves the profile, builds a deploy plan and (unless
    /// <see cref="DeployOptions.DryRun"/> is set) applies the plan to disk.
    /// </summary>
    DeployResult Deploy(string sourceRepoRoot, DeployOptions options);
}
