using AiSetupLib.Models;

namespace AiSetupLib.Targets;

public interface IDeployTarget
{
    DeployTarget Target { get; }
    DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options);
    void Apply(DeployPlan plan, IReadOnlyList<AssetDefinition> assets, DeployOptions options);
}
