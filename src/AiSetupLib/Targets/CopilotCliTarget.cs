using AiSetup.Lib.Aggregation;
using AiSetup.Lib.IO;
using AiSetup.Lib.Models;
using AiSetup.Lib.Targets.Platform;
using CreativeCoders.Core;

namespace AiSetup.Lib.Targets;

/// <summary>
/// Deploy target for GitHub Copilot CLI.
/// </summary>
public sealed class CopilotCliTarget : BaseDeployTarget
{
    private const string McpFileName = "mcp.json";
    private const string McpRootKey = "servers";

    private readonly IMcpConfigMerger _mcpMerger;

    /// <summary>Initialises a new instance.</summary>
    public CopilotCliTarget(
        IFileSystem fileSystem,
        IPlatformPathProvider pathProvider,
        IMcpConfigMerger mcpMerger)
        : base(fileSystem, pathProvider)
    {
        _mcpMerger = Ensure.NotNull(mcpMerger, nameof(mcpMerger));
    }

    /// <inheritdoc />
    public override DeployTarget Target => DeployTarget.CopilotCli;

    /// <inheritdoc />
    protected override IEnumerable<DeployAction> BuildActions(
        IReadOnlyList<AssetDefinition> assets,
        DeployOptions options,
        string rootDirectory)
    {
        var (instructionsDir, agentsDir, skillsDir, mcpPath) = ResolveLayout(options, rootDirectory);

        foreach (var asset in assets.Where(a => a.Type == AssetType.Instruction))
        {
            var fileName = Path.GetFileName(asset.AbsolutePath);
            var targetPath = Path.Combine(instructionsDir, fileName);

            yield return new DeployAction(
                Kind: ClassifyKind(targetPath, isDirectory: false),
                SourcePath: asset.AbsolutePath,
                TargetPath: targetPath,
                Content: asset.RawContent,
                IsDirectoryCopy: false,
                Reason: null);
        }

        foreach (var asset in assets.Where(a => a.Type == AssetType.Agent))
        {
            var fileName = Path.GetFileName(asset.AbsolutePath);
            var targetPath = Path.Combine(agentsDir, fileName);

            yield return new DeployAction(
                Kind: ClassifyKind(targetPath, isDirectory: false),
                SourcePath: asset.AbsolutePath,
                TargetPath: targetPath,
                Content: asset.RawContent,
                IsDirectoryCopy: false,
                Reason: null);
        }

        foreach (var asset in assets.Where(a => a.Type == AssetType.Skill))
        {
            var sourceDir = Path.GetDirectoryName(asset.AbsolutePath)!;
            var targetPath = Path.Combine(skillsDir, asset.Name);

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
            var existing = FileSystem.FileExists(mcpPath) ? FileSystem.ReadAllText(mcpPath) : null;
            var content = _mcpMerger.Merge(mcpAssets, existing, McpRootKey);

            yield return new DeployAction(
                Kind: ClassifyKind(mcpPath, isDirectory: false),
                SourcePath: null,
                TargetPath: mcpPath,
                Content: content,
                IsDirectoryCopy: false,
                Reason: null);
        }
    }

    private static (string Instructions, string Agents, string Skills, string McpPath) ResolveLayout(
        DeployOptions options,
        string rootDirectory)
    {
        if (options.Mode == DeployMode.Repo)
        {
            var github = Path.Combine(rootDirectory, ".github");

            return (
                Instructions: Path.Combine(github, "instructions"),
                Agents: Path.Combine(github, "agents"),
                Skills: Path.Combine(github, "skills"),
                McpPath: Path.Combine(rootDirectory, ".vscode", McpFileName));
        }

        return (
            Instructions: Path.Combine(rootDirectory, "instructions"),
            Agents: Path.Combine(rootDirectory, "agents"),
            Skills: Path.Combine(rootDirectory, "skills"),
            McpPath: Path.Combine(rootDirectory, McpFileName));
    }
}
