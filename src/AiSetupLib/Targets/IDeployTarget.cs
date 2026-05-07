using AiSetup.Models;
using AiSetup.Profiles;

namespace AiSetup.Targets;

/// <summary>
/// Adapter that knows how to translate a set of resolved assets into concrete
/// file operations for a particular target system.
/// </summary>
public interface IDeployTarget
{
    /// <summary>The target system this adapter handles.</summary>
    DeployTarget Target { get; }

    /// <summary>Builds a deploy plan without performing any writes.</summary>
    /// <param name="options">Deploy options (target/mode/force/dry-run).</param>
    /// <param name="assets">Assets resolved from profile + overrides.</param>
    /// <returns>Plan of actions for the deploy service to execute.</returns>
    DeployPlan Plan(DeployOptions options, ResolvedAssets assets);
}
