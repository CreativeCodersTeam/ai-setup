namespace AiSetup.Models;

/// <summary>
/// Outcome of a deploy run. Mirrors <see cref="DeployPlan"/> with execution results.
/// </summary>
/// <param name="Plan">The plan that was executed (or would have been executed in dry-run).</param>
/// <param name="Executed">Actions that were executed successfully.</param>
/// <param name="Skipped">Actions that were skipped (e.g. existing files without --force).</param>
/// <param name="Errors">Errors that occurred during execution, paired with the offending action.</param>
/// <param name="DryRun">True if no actions were actually executed.</param>
public sealed record DeployReport(
    DeployPlan Plan,
    IReadOnlyList<DeployAction> Executed,
    IReadOnlyList<DeployAction> Skipped,
    IReadOnlyList<(DeployAction Action, string Error)> Errors,
    bool DryRun);
