using AiSetup.Models;

namespace AiSetup.Deploy;

/// <summary>
/// High-level entry point: resolves profile + assets, asks the appropriate target for a plan
/// and executes it (unless dry-run is requested).
/// </summary>
public interface IDeployService
{
    /// <summary>Runs the deploy described by <paramref name="options"/>.</summary>
    /// <param name="options">Deploy options.</param>
    /// <returns>Report describing executed and skipped actions.</returns>
    DeployReport Deploy(DeployOptions options);
}
