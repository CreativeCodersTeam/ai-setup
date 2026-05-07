using AiSetup.Lib.Models;

namespace AiSetup.Lib.Targets;

/// <summary>
/// Adapter that materialises a list of assets into the file layout expected by
/// a specific AI system.
/// </summary>
public interface IDeployTarget
{
    /// <summary>The AI system this target serves.</summary>
    DeployTarget Target { get; }

    /// <summary>
    /// Produces the planned set of file actions for the given assets and
    /// options without writing anything to disk.
    /// </summary>
    DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options);
}
