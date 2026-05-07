using AiSetup.Exceptions;
using AiSetup.Models;
using CreativeCoders.Core;

namespace AiSetup.Targets;

/// <summary>
/// Default <see cref="ITargetRegistry"/>: resolves a target adapter by enum value
/// by querying the registered <see cref="IDeployTarget"/> instances.
/// </summary>
public sealed class TargetRegistry : ITargetRegistry
{
    private readonly Dictionary<DeployTarget, IDeployTarget> _targets;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="targets">All <see cref="IDeployTarget"/> implementations registered in DI.</param>
    public TargetRegistry(IEnumerable<IDeployTarget> targets)
    {
        Ensure.NotNull(targets);
        _targets = targets.ToDictionary(t => t.Target);
    }

    /// <inheritdoc />
    public IDeployTarget Get(DeployTarget target)
    {
        return _targets.TryGetValue(target, out var adapter)
            ? adapter
            : throw new AiSetupException($"No deploy target registered for '{target}'.");
    }
}
