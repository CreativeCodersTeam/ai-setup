using AiSetup.Aggregation;
using AiSetup.Models;
using AiSetup.Platform;

namespace AiSetup.Targets;

/// <summary>
/// Deploy target for Anthropic Claude Code. Aggregates instructions into <c>CLAUDE.md</c>,
/// writes agent and skill folders into <c>.claude/</c>, and merges MCP servers into
/// <c>.claude/settings.json</c>.
/// </summary>
public sealed class ClaudeCodeTarget : DeployTargetBase
{
    private const string ClaudeMdFile = "CLAUDE.md";
    private const string BackupSuffix = ".bak";
    private const string ClaudeFolder = ".claude";
    private const string SettingsFile = "settings.json";

    /// <summary>Initializes a new instance.</summary>
    /// <param name="fileSystem">File system abstraction.</param>
    /// <param name="pathProvider">OS-specific path provider.</param>
    /// <param name="markdownAggregator">Markdown aggregator.</param>
    /// <param name="mcpConfigMerger">MCP merger.</param>
    public ClaudeCodeTarget(
        IFileSystem fileSystem,
        IPathProvider pathProvider,
        IMarkdownAggregator markdownAggregator,
        IMcpConfigMerger mcpConfigMerger)
        : base(fileSystem, pathProvider, markdownAggregator, mcpConfigMerger)
    {
    }

    /// <inheritdoc />
    public override DeployTarget Target => DeployTarget.ClaudeCode;

    /// <inheritdoc />
    protected override void PlanInstructions(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> instructions, string root)
    {
        if (instructions.Count == 0)
        {
            return;
        }

        var targetPath = Path.Combine(root, ClaudeMdFile);

        if (options.Mode == DeployMode.Local && FileSystem.FileExists(targetPath))
        {
            var backupPath = targetPath + BackupSuffix;
            actions.Add(new BackupFileAction(
                backupPath,
                targetPath,
                StatusFor(backupPath),
                $"Backup existing CLAUDE.md to {Path.GetFileName(backupPath)}"));
        }

        var content = MarkdownAggregator.Aggregate(instructions);
        actions.Add(new WriteFileAction(
            targetPath,
            content,
            StatusFor(targetPath),
            "Aggregated CLAUDE.md"));
    }

    /// <inheritdoc />
    protected override void PlanAgents(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> agents, string root)
    {
        var folder = Path.Combine(ResolveClaudeBase(root, options), "agents");

        foreach (var agent in agents)
        {
            var targetPath = Path.Combine(folder, LeafId(agent.Id) + ".md");
            actions.Add(new WriteFileAction(
                targetPath,
                agent.Body,
                StatusFor(targetPath),
                $"Agent '{agent.Id}'"));
        }
    }

    /// <inheritdoc />
    protected override void PlanSkills(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> skills, string root)
    {
        var folder = Path.Combine(ResolveClaudeBase(root, options), "skills");

        foreach (var asset in skills)
        {
            if (asset is not SkillAsset skill)
            {
                continue;
            }

            var targetPath = Path.Combine(folder, LeafId(skill.Id));
            actions.Add(new CopyDirectoryAction(
                skill.Folder,
                targetPath,
                StatusFor(targetPath),
                $"Skill '{skill.Id}'"));
        }
    }

    /// <inheritdoc />
    protected override void PlanMcpConfigs(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> mcpConfigs, string root)
    {
        if (mcpConfigs.Count == 0)
        {
            return;
        }

        var targetPath = Path.Combine(ResolveClaudeBase(root, options), SettingsFile);
        var existing = FileSystem.FileExists(targetPath) ? FileSystem.ReadAllText(targetPath) : null;
        var content = McpConfigMerger.Merge(mcpConfigs, McpServersKey.ClaudeCode, existing, options.Force);

        actions.Add(new WriteFileAction(
            targetPath,
            content,
            StatusFor(targetPath),
            "MCP servers (.claude/settings.json)"));
    }

    private static string ResolveClaudeBase(string root, DeployOptions options)
        => options.Mode == DeployMode.Repo ? Path.Combine(root, ClaudeFolder) : root;

    private static string LeafId(string id)
    {
        var slash = id.LastIndexOf('/');
        return slash < 0 ? id : id[(slash + 1)..];
    }
}
