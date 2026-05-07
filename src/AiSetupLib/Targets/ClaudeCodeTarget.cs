using AiSetup.Lib.Aggregation;
using AiSetup.Lib.IO;
using AiSetup.Lib.Models;
using AiSetup.Lib.Targets.Platform;
using CreativeCoders.Core;

namespace AiSetup.Lib.Targets;

/// <summary>
/// Deploy target for Anthropic Claude Code.
/// </summary>
public sealed class ClaudeCodeTarget : BaseDeployTarget
{
    private const string ClaudeMdFileName = "CLAUDE.md";
    private const string SettingsFileName = "settings.json";
    private const string McpRootKey = "mcpServers";

    private readonly IContentAggregator _aggregator;
    private readonly IMcpConfigMerger _mcpMerger;

    /// <summary>Initialises a new instance.</summary>
    public ClaudeCodeTarget(
        IFileSystem fileSystem,
        IPlatformPathProvider pathProvider,
        IContentAggregator aggregator,
        IMcpConfigMerger mcpMerger)
        : base(fileSystem, pathProvider)
    {
        _aggregator = Ensure.NotNull(aggregator, nameof(aggregator));
        _mcpMerger = Ensure.NotNull(mcpMerger, nameof(mcpMerger));
    }

    /// <inheritdoc />
    public override DeployTarget Target => DeployTarget.ClaudeCode;

    /// <inheritdoc />
    protected override IEnumerable<DeployAction> BuildActions(
        IReadOnlyList<AssetDefinition> assets,
        DeployOptions options,
        string rootDirectory)
    {
        var (claudeMdPath, commandsDir, settingsPath) = ResolveLayout(options, rootDirectory);

        var aggregatable = assets
            .Where(a => a.Type is AssetType.Instruction or AssetType.Agent)
            .ToArray();

        if (aggregatable.Length > 0)
        {
            var content = _aggregator.Aggregate(aggregatable);

            yield return new DeployAction(
                Kind: ClassifyKind(claudeMdPath, isDirectory: false),
                SourcePath: null,
                TargetPath: claudeMdPath,
                Content: content,
                IsDirectoryCopy: false,
                Reason: null);
        }

        foreach (var skill in assets.Where(a => a.Type == AssetType.Skill))
        {
            var sourceDir = Path.GetDirectoryName(skill.AbsolutePath)!;
            var targetPath = Path.Combine(commandsDir, skill.Name);

            yield return new DeployAction(
                Kind: ClassifyKind(targetPath, isDirectory: true),
                SourcePath: sourceDir,
                TargetPath: targetPath,
                Content: null,
                IsDirectoryCopy: true,
                Reason: null);
        }

        var mcpAssets = assets.Where(a => a.Type == AssetType.McpConfig).ToArray();

        if (mcpAssets.Length > 0 && options.IncludeMcpConfigs)
        {
            var existing = FileSystem.FileExists(settingsPath) ? FileSystem.ReadAllText(settingsPath) : null;
            var content = _mcpMerger.Merge(mcpAssets, existing, McpRootKey);

            yield return new DeployAction(
                Kind: ClassifyKind(settingsPath, isDirectory: false),
                SourcePath: null,
                TargetPath: settingsPath,
                Content: content,
                IsDirectoryCopy: false,
                Reason: null);
        }
    }

    private static (string ClaudeMd, string CommandsDir, string SettingsPath) ResolveLayout(
        DeployOptions options,
        string rootDirectory)
    {
        if (options.Mode == DeployMode.Repo)
        {
            return (
                ClaudeMd: Path.Combine(rootDirectory, ClaudeMdFileName),
                CommandsDir: Path.Combine(rootDirectory, ".claude", "commands"),
                SettingsPath: Path.Combine(rootDirectory, ".claude", SettingsFileName));
        }

        return (
            ClaudeMd: Path.Combine(rootDirectory, ClaudeMdFileName),
            CommandsDir: Path.Combine(rootDirectory, "commands"),
            SettingsPath: Path.Combine(rootDirectory, SettingsFileName));
    }
}
