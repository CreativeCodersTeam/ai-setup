using AiSetup.Aggregation;
using AiSetup.Models;
using AiSetup.Platform;

namespace AiSetup.Targets;

/// <summary>
/// Deploy target for GitHub Copilot CLI. Writes individual files / folders into
/// <c>.github/</c> (repo mode) or the OS-specific local config root.
/// </summary>
public sealed class CopilotCliTarget : DeployTargetBase
{
    /// <summary>Initializes a new instance.</summary>
    /// <param name="fileSystem">File system abstraction.</param>
    /// <param name="pathProvider">OS-specific path provider.</param>
    /// <param name="markdownAggregator">Markdown aggregator (unused but injected for symmetry).</param>
    /// <param name="mcpConfigMerger">MCP merger.</param>
    public CopilotCliTarget(
        IFileSystem fileSystem,
        IPathProvider pathProvider,
        IMarkdownAggregator markdownAggregator,
        IMcpConfigMerger mcpConfigMerger)
        : base(fileSystem, pathProvider, markdownAggregator, mcpConfigMerger)
    {
    }

    /// <inheritdoc />
    public override DeployTarget Target => DeployTarget.CopilotCli;

    /// <inheritdoc />
    protected override void PlanInstructions(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> instructions, string root)
    {
        var folder = options.Mode == DeployMode.Repo
            ? Path.Combine(root, ".github", "instructions")
            : Path.Combine(root, "instructions");

        foreach (var asset in instructions)
        {
            var targetPath = Path.Combine(folder, LeafId(asset.Id) + ".md");
            actions.Add(new WriteFileAction(
                targetPath,
                asset.Body,
                StatusFor(targetPath),
                $"Instruction '{asset.Id}'"));
        }
    }

    /// <inheritdoc />
    protected override void PlanAgents(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> agents, string root)
    {
        var folder = options.Mode == DeployMode.Repo
            ? Path.Combine(root, ".github", "agents")
            : Path.Combine(root, "agents");

        foreach (var asset in agents)
        {
            var targetPath = Path.Combine(folder, LeafId(asset.Id) + ".md");
            actions.Add(new WriteFileAction(
                targetPath,
                asset.Body,
                StatusFor(targetPath),
                $"Agent '{asset.Id}'"));
        }
    }

    /// <inheritdoc />
    protected override void PlanSkills(
        List<DeployAction> actions, DeployOptions options, IReadOnlyList<AssetDefinition> skills, string root)
    {
        var folder = options.Mode == DeployMode.Repo
            ? Path.Combine(root, ".github", "skills")
            : Path.Combine(root, "skills");

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

        if (options.Mode == DeployMode.Local)
        {
            actions.Add(new WriteFileAction(
                Path.Combine(root, "mcp.json"),
                BuildMcpJson(mcpConfigs, options, existingPath: Path.Combine(root, "mcp.json")),
                StatusFor(Path.Combine(root, "mcp.json")),
                "MCP servers (local mcp.json)"));
            return;
        }

        var targetPath = Path.Combine(root, ".vscode", "mcp.json");
        actions.Add(new WriteFileAction(
            targetPath,
            BuildMcpJson(mcpConfigs, options, existingPath: targetPath),
            StatusFor(targetPath),
            "MCP servers (.vscode/mcp.json)"));
    }

    private string BuildMcpJson(IReadOnlyList<AssetDefinition> configs, DeployOptions options, string existingPath)
    {
        var existing = FileSystem.FileExists(existingPath) ? FileSystem.ReadAllText(existingPath) : null;
        return McpConfigMerger.Merge(configs, McpServersKey.CopilotCli, existing, options.Force);
    }

    private static string LeafId(string id)
    {
        var slash = id.LastIndexOf('/');
        return slash < 0 ? id : id[(slash + 1)..];
    }
}
