using AiSetup.Lib.Discovery;
using AiSetup.Lib.Exceptions;
using AiSetup.Lib.IO;
using AiSetup.Lib.Models;
using AiSetup.Lib.Profiles;
using AiSetup.Lib.Targets;
using AiSetup.Lib.Util;
using CreativeCoders.Core;

namespace AiSetup.Lib.Deploy;

/// <inheritdoc cref="IDeployService"/>
public sealed class DeployService : IDeployService
{
    private readonly IAssetDiscovery _discovery;
    private readonly IProfileResolver _profileResolver;
    private readonly TargetRegistry _targets;
    private readonly IFileSystem _fileSystem;

    /// <summary>Initialises a new instance.</summary>
    public DeployService(
        IAssetDiscovery discovery,
        IProfileResolver profileResolver,
        TargetRegistry targets,
        IFileSystem fileSystem)
    {
        _discovery = Ensure.NotNull(discovery, nameof(discovery));
        _profileResolver = Ensure.NotNull(profileResolver, nameof(profileResolver));
        _targets = Ensure.NotNull(targets, nameof(targets));
        _fileSystem = Ensure.NotNull(fileSystem, nameof(fileSystem));
    }

    /// <inheritdoc />
    public DeployResult Deploy(string sourceRepoRoot, DeployOptions options)
    {
        Ensure.IsNotNullOrWhitespace(sourceRepoRoot, nameof(sourceRepoRoot));
        Ensure.NotNull(options, nameof(options));

        var warnings = new List<string>();
        var allAssets = _discovery.Discover(sourceRepoRoot, warnings);

        var selection = ResolveSelection(sourceRepoRoot, options, allAssets);
        var target = _targets.Resolve(options.Target);
        var plan = target.Plan(selection, options);

        if (options.DryRun)
        {
            return new DeployResult(plan, warnings, Applied: false);
        }

        Apply(plan, options.Force);

        return new DeployResult(plan, warnings, Applied: true);
    }

    private IReadOnlyList<AssetDefinition> ResolveSelection(
        string sourceRepoRoot,
        DeployOptions options,
        IReadOnlyList<AssetDefinition> all)
    {
        var requested = new HashSet<(AssetType Type, string Name)>();

        if (!string.IsNullOrWhiteSpace(options.Profile))
        {
            var profile = _profileResolver.LoadProfile(sourceRepoRoot, options.Profile)
                ?? throw new MissingAssetException(
                    options.Profile,
                    LevenshteinSuggester.Suggest(
                        options.Profile,
                        _profileResolver.ListProfiles(sourceRepoRoot).Select(p => p.Name)));

            AddRequested(requested, AssetType.Agent, profile.Agents);
            AddRequested(requested, AssetType.Instruction, profile.Instructions);
            AddRequested(requested, AssetType.Skill, profile.Skills);
            AddRequested(requested, AssetType.McpConfig, profile.McpConfigs);
        }

        AddRequested(requested, AssetType.Agent, options.Agents);
        AddRequested(requested, AssetType.Instruction, options.Instructions);
        AddRequested(requested, AssetType.Skill, options.Skills);
        AddRequested(requested, AssetType.McpConfig, options.McpConfigs);

        if (requested.Count == 0)
        {
            return Array.Empty<AssetDefinition>();
        }

        var matched = new List<AssetDefinition>();
        var unmatched = new List<(AssetType Type, string Name)>();

        foreach (var (type, name) in requested)
        {
            var asset = FindAsset(all, type, name);

            if (asset is null)
            {
                unmatched.Add((type, name));
            }
            else
            {
                matched.Add(asset);
            }
        }

        if (unmatched.Count > 0)
        {
            var first = unmatched[0];
            var candidates = all
                .Where(a => a.Type == first.Type)
                .Select(a => a.Name);

            throw new MissingAssetException(first.Name, LevenshteinSuggester.Suggest(first.Name, candidates));
        }

        return matched;
    }

    private static void AddRequested(
        HashSet<(AssetType Type, string Name)> set,
        AssetType type,
        IEnumerable<string>? names)
    {
        if (names is null)
        {
            return;
        }

        foreach (var raw in names)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            set.Add((type, NormaliseName(raw)));
        }
    }

    private static string NormaliseName(string raw)
    {
        var trimmed = raw.Trim();
        var lastSlash = trimmed.LastIndexOf('/');

        return lastSlash >= 0 ? trimmed[(lastSlash + 1)..] : trimmed;
    }

    private static AssetDefinition? FindAsset(
        IReadOnlyList<AssetDefinition> all,
        AssetType type,
        string name)
    {
        var stripped = name.EndsWith(".instructions", StringComparison.OrdinalIgnoreCase)
            ? name[..^".instructions".Length]
            : name;

        return all.FirstOrDefault(a =>
            a.Type == type
            && (a.Name.Equals(name, StringComparison.OrdinalIgnoreCase)
                || a.Name.Equals(stripped, StringComparison.OrdinalIgnoreCase)));
    }

    private void Apply(DeployPlan plan, bool force)
    {
        foreach (var action in plan.Actions)
        {
            switch (action.Kind)
            {
                case DeployActionKind.Create:
                case DeployActionKind.Overwrite:
                    if (action.Kind == DeployActionKind.Overwrite && !force)
                    {
                        continue;
                    }

                    if (action.IsDirectoryCopy)
                    {
                        _fileSystem.CopyDirectory(action.SourcePath!, action.TargetPath, overwrite: force);
                    }
                    else
                    {
                        _fileSystem.WriteAllText(action.TargetPath, action.Content ?? string.Empty);
                    }

                    break;
                case DeployActionKind.Skip:
                case DeployActionKind.Error:
                    break;
                default:
                    throw new InvalidOperationException($"Unhandled deploy action kind: {action.Kind}");
            }
        }
    }
}
