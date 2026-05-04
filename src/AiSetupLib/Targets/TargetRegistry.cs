using AiSetupLib.Models;

namespace AiSetupLib.Targets;

public sealed class TargetRegistry
{
    private readonly Dictionary<DeployTarget, IDeployTarget> _byTarget;

    public TargetRegistry(IEnumerable<IDeployTarget> targets)
    {
        _byTarget = targets.ToDictionary(t => t.Target);
    }

    public IDeployTarget Get(DeployTarget target)
    {
        if (_byTarget.TryGetValue(target, out var t)) return t;
        throw new InvalidOperationException($"No IDeployTarget registered for {target}");
    }
}
