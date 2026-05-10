using AiSetup.Aggregation;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using CreativeCoders.Core;

namespace AiSetup.Targets;

/// <summary>
/// Common scaffolding for <see cref="IDeployTarget"/> implementations.
/// </summary>
public abstract class DeployTargetBase : IDeployTarget
{
    /// <summary>File system used to detect existing files for status determination.</summary>
    protected IFileSystem FileSystem { get; }

    /// <summary>OS-specific path provider for local-mode roots.</summary>
    protected IPathProvider PathProvider { get; }

    /// <summary>Aggregator used by targets that combine markdown.</summary>
    protected IMarkdownAggregator MarkdownAggregator { get; }

    /// <summary>Merger used by targets that produce JSON MCP settings.</summary>
    protected IMcpConfigMerger McpConfigMerger { get; }

    /// <summary>Merger used by targets that produce JSON settings files.</summary>
    protected ISettingsMerger SettingsMerger { get; }

    /// <summary>Initializes shared dependencies.</summary>
    /// <param name="fileSystem">File system abstraction.</param>
    /// <param name="pathProvider">OS-specific path provider.</param>
    /// <param name="markdownAggregator">Markdown aggregator.</param>
    /// <param name="mcpConfigMerger">MCP merger.</param>
    /// <param name="settingsMerger">Settings fragment merger.</param>
    protected DeployTargetBase(
        IFileSystem fileSystem,
        IPathProvider pathProvider,
        IMarkdownAggregator markdownAggregator,
        IMcpConfigMerger mcpConfigMerger,
        ISettingsMerger settingsMerger)
    {
        FileSystem = Ensure.NotNull(fileSystem);
        PathProvider = Ensure.NotNull(pathProvider);
        MarkdownAggregator = Ensure.NotNull(markdownAggregator);
        McpConfigMerger = Ensure.NotNull(mcpConfigMerger);
        SettingsMerger = Ensure.NotNull(settingsMerger);
    }

    /// <inheritdoc />
    public abstract DeployTarget Target { get; }

    /// <inheritdoc />
    public DeployPlan Plan(DeployOptions options, ResolvedAssets assets)
    {
        Ensure.NotNull(options);
        Ensure.NotNull(assets);

        var actions = new List<DeployAction>();

        foreach (var mode in Enum.GetValues<DeployMode>())
        {
            var instructions = FilterByMode(assets.Instructions, mode);
            var agents = FilterByMode(assets.Agents, mode);
            var skills = FilterByMode(assets.Skills, mode);
            var mcpConfigs = FilterByMode(assets.McpConfigs, mode);
            var settings = FilterSettingsForTarget(assets.Settings, mode);

            if (instructions.Count == 0 && agents.Count == 0 && skills.Count == 0
                && mcpConfigs.Count == 0 && settings.Count == 0)
            {
                continue;
            }

            var root = ResolveRoot(options, mode);

            PlanInstructions(actions, options, mode, instructions, root);
            PlanAgents(actions, options, mode, agents, root);
            PlanSkills(actions, options, mode, skills, root);
            PlanMcpConfigs(actions, options, mode, mcpConfigs, root);
            PlanSettings(actions, options, mode, settings, root);
        }

        return new DeployPlan(Target, actions);
    }

    private static IReadOnlyList<AssetDefinition> FilterByMode(IReadOnlyList<ResolvedAsset> assets, DeployMode mode)
    {
        var result = new List<AssetDefinition>();

        foreach (var asset in assets)
        {
            if (asset.Mode == mode)
            {
                result.Add(asset.Definition);
            }
        }

        return result;
    }

    private IReadOnlyList<AssetDefinition> FilterSettingsForTarget(
        IReadOnlyList<ResolvedAsset> assets, DeployMode mode)
    {
        var result = new List<AssetDefinition>();

        foreach (var asset in assets)
        {
            if (asset.Mode == mode && asset.Definition.Targets.Contains(Target))
            {
                result.Add(asset.Definition);
            }
        }

        return result;
    }

    /// <summary>Resolves the destination root directory for the given deploy mode.</summary>
    protected virtual string ResolveRoot(DeployOptions options, DeployMode mode)
    {
        Ensure.NotNull(options);

        if (mode == DeployMode.Repo)
        {
            return options.DestinationRepoPath
                ?? throw new AiSetupException(
                    "DestinationRepoPath is required when the profile contains a 'repo'-mode asset.");
        }

        return PathProvider.GetLocalRoot(Target);
    }

    /// <summary>Determines whether a planned write would create or overwrite the target.</summary>
    protected DeployActionStatus StatusFor(string targetPath)
    {
        return FileSystem.FileExists(targetPath) || FileSystem.DirectoryExists(targetPath)
            ? DeployActionStatus.Overwrite
            : DeployActionStatus.Create;
    }

    /// <summary>Plans how instruction assets are deployed for one deploy mode.</summary>
    protected abstract void PlanInstructions(
        List<DeployAction> actions, DeployOptions options, DeployMode mode,
        IReadOnlyList<AssetDefinition> instructions, string root);

    /// <summary>Plans how agent assets are deployed for one deploy mode.</summary>
    protected abstract void PlanAgents(
        List<DeployAction> actions, DeployOptions options, DeployMode mode,
        IReadOnlyList<AssetDefinition> agents, string root);

    /// <summary>Plans how skill assets are deployed for one deploy mode.</summary>
    protected abstract void PlanSkills(
        List<DeployAction> actions, DeployOptions options, DeployMode mode,
        IReadOnlyList<AssetDefinition> skills, string root);

    /// <summary>Plans how MCP config assets are deployed for one deploy mode.</summary>
    protected abstract void PlanMcpConfigs(
        List<DeployAction> actions, DeployOptions options, DeployMode mode,
        IReadOnlyList<AssetDefinition> mcpConfigs, string root);

    /// <summary>Plans how settings fragments are deep-merged into the target's settings file for one deploy mode.</summary>
    protected abstract void PlanSettings(
        List<DeployAction> actions, DeployOptions options, DeployMode mode,
        IReadOnlyList<AssetDefinition> settings, string root);
}
