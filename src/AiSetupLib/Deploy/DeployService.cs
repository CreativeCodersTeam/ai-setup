using AiSetupLib.Discovery;
using AiSetupLib.Models;
using AiSetupLib.Profiles;
using AiSetupLib.Targets;

namespace AiSetupLib.Deploy;

public sealed class DeployService : IDeployService
{
    private readonly IAssetDiscovery _discovery;
    private readonly IProfileResolver _profiles;
    private readonly TargetRegistry _registry;
    private readonly string _repoRoot;

    public DeployService(
        IAssetDiscovery discovery,
        IProfileResolver profiles,
        TargetRegistry registry,
        string repoRoot)
    {
        _discovery = discovery;
        _profiles = profiles;
        _registry = registry;
        _repoRoot = repoRoot;
    }

    public DeployPlan Deploy(DeployOptions options)
    {
        var requested = ResolveRequested(options);
        var available = _discovery.Discover(_repoRoot);

        var byName = available.ToDictionary(a => a.Name, StringComparer.Ordinal);
        var resolved = new List<AssetDefinition>();
        var missing = new List<string>();

        foreach (var name in requested)
        {
            if (byName.TryGetValue(name, out var asset)) resolved.Add(asset);
            else missing.Add(name);
        }

        if (missing.Count > 0)
        {
            var allNames = available.Select(a => a.Name).ToList();
            var suggestions = missing.ToDictionary(
                m => m,
                m => Suggestions.Closest(m, allNames));
            throw new AssetNotFoundException(missing, suggestions);
        }

        var target = _registry.Get(options.Target);
        var plan = target.Plan(resolved, options);
        if (!options.DryRun)
            target.Apply(plan, resolved, options);
        return plan;
    }

    private IReadOnlyList<string> ResolveRequested(DeployOptions options)
    {
        var names = new List<string>();
        if (options.Profile is not null)
        {
            var profile = _profiles.Resolve(_repoRoot, options.Profile);
            names.AddRange(profile.Agents);
            names.AddRange(profile.Instructions);
            names.AddRange(profile.Skills);
            names.AddRange(profile.McpConfigs);
        }
        names.AddRange(options.Agents);
        names.AddRange(options.Instructions);
        names.AddRange(options.Skills);
        names.AddRange(options.McpConfigs);
        return names.Distinct(StringComparer.Ordinal).ToList();
    }
}
