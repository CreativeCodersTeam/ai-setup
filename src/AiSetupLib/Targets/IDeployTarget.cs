using AiSetupLib.Models;

namespace AiSetupLib.Targets;

public interface IDeployTarget
{
    /// <summary>
    /// Identifies which deploy target (e.g. CopilotCli, ClaudeCode) this implementation handles.
    /// </summary>
    DeployTarget Target { get; }

    /// <summary>
    /// Compute the deploy plan describing what actions would be applied.
    /// </summary>
    /// <remarks>
    /// <see cref="Plan"/> performs read-only filesystem I/O (existence checks)
    /// to distinguish Create from Overwrite actions. Safe to call without
    /// modifying any files; pair with <see cref="Apply"/> to actually write.
    /// </remarks>
    DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options);

    /// <summary>
    /// Execute the given plan, writing assets to their resolved destinations.
    /// </summary>
    void Apply(DeployPlan plan, IReadOnlyList<AssetDefinition> assets, DeployOptions options);
}
