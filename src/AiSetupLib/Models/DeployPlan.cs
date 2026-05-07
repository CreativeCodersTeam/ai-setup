namespace AiSetup.Models;

/// <summary>
/// Ordered list of actions a deploy target intends to perform.
/// </summary>
/// <param name="Target">Target system the plan is built for.</param>
/// <param name="Mode">Repo or local mode the plan is built for.</param>
/// <param name="Actions">Ordered actions to execute.</param>
public sealed record DeployPlan(DeployTarget Target, DeployMode Mode, IReadOnlyList<DeployAction> Actions);
