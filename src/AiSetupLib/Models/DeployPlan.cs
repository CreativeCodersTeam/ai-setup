namespace AiSetupLib.Models;

public sealed record DeployPlan(IReadOnlyList<DeployAction> Actions);
