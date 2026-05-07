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

    /// <summary>Initializes shared dependencies.</summary>
    /// <param name="fileSystem">File system abstraction.</param>
    /// <param name="pathProvider">OS-specific path provider.</param>
    /// <param name="markdownAggregator">Markdown aggregator.</param>
    /// <param name="mcpConfigMerger">MCP merger.</param>
    protected DeployTargetBase(
        IFileSystem fileSystem,
        IPathProvider pathProvider,
        IMarkdownAggregator markdownAggregator,
        IMcpConfigMerger mcpConfigMerger)
    {
        FileSystem = Ensure.NotNull(fileSystem);
        PathProvider = Ensure.NotNull(pathProvider);
        MarkdownAggregator = Ensure.NotNull(markdownAggregator);
        McpConfigMerger = Ensure.NotNull(mcpConfigMerger);
    }

    /// <inheritdoc />
    public abstract DeployTarget Target { get; }

    /// <inheritdoc />
    public DeployPlan Plan(DeployOptions options, ResolvedAssets assets)
    {
        Ensure.NotNull(options);
        Ensure.NotNull(assets);

        var root = ResolveRoot(options);
        var actions = new List<DeployAction>();

        PlanInstructions(actions, options, assets.Instructions, root);
        PlanAgents(actions, options, assets.Agents, root);
        PlanSkills(actions, options, assets.Skills, root);
        PlanMcpConfigs(actions, options, assets.McpConfigs, root);

        return new DeployPlan(Target, options.Mode, actions);
    }

    /// <summary>Resolves the destination root directory based on mode.</summary>
    protected virtual string ResolveRoot(DeployOptions options)
    {
        if (options.Mode == DeployMode.Repo)
        {
            return options.DestinationRepoPath
                ?? throw new AiSetupException("DestinationRepoPath is required when Mode is Repo.");
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

    /// <summary>Plans how instruction assets are deployed.</summary>
    protected abstract void PlanInstructions(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> instructions, string root);

    /// <summary>Plans how agent assets are deployed.</summary>
    protected abstract void PlanAgents(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> agents, string root);

    /// <summary>Plans how skill assets are deployed.</summary>
    protected abstract void PlanSkills(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> skills, string root);

    /// <summary>Plans how MCP config assets are deployed.</summary>
    protected abstract void PlanMcpConfigs(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> mcpConfigs, string root);
}
