namespace AiSetup.Models;

/// <summary>
/// Ordered list of actions a deploy target intends to perform. Actions may target both the
/// destination repository and the local config root when the profile mixes deploy modes.
/// </summary>
/// <param name="Target">Target system the plan is built for.</param>
/// <param name="Actions">Ordered actions to execute.</param>
public sealed record DeployPlan(DeployTarget Target, IReadOnlyList<DeployAction> Actions);
