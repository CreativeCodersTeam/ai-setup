using AiSetup.Lib.Models;
using CreativeCoders.Core;

namespace AiSetup.Lib.Targets;

/// <summary>
/// Look-up by <see cref="DeployTarget"/> for the registered
/// <see cref="IDeployTarget"/> implementation.
/// </summary>
public sealed class TargetRegistry
{
    private readonly IReadOnlyDictionary<DeployTarget, IDeployTarget> _targets;

    /// <summary>Initialises a new instance from the DI-registered targets.</summary>
    public TargetRegistry(IEnumerable<IDeployTarget> targets)
    {
        Ensure.NotNull(targets, nameof(targets));
        _targets = targets.ToDictionary(t => t.Target);
    }

    /// <summary>Resolves the target adapter for <paramref name="target"/>.</summary>
    public IDeployTarget Resolve(DeployTarget target)
    {
        if (_targets.TryGetValue(target, out var found))
        {
            return found;
        }

        throw new InvalidOperationException($"No deploy target registered for '{target}'.");
    }
}
