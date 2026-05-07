namespace AiSetup.Lib.Models;

/// <summary>
/// The set of <see cref="DeployAction"/>s a target plans to execute.
/// </summary>
/// <param name="Target">Target system the plan was produced for.</param>
/// <param name="Mode">Deploy mode the plan applies to.</param>
/// <param name="Actions">Ordered list of planned actions.</param>
public sealed record DeployPlan(
    DeployTarget Target,
    DeployMode Mode,
    IReadOnlyList<DeployAction> Actions);
