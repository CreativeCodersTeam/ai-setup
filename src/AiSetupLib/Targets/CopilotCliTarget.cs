using System.IO.Abstractions;
using System.Text.Json;
using AiSetupLib.Aggregation;
using AiSetupLib.Models;
using AiSetupLib.Paths;

namespace AiSetupLib.Targets;

public sealed class CopilotCliTarget : IDeployTarget
{
    private readonly IFileSystem _fs;
    private readonly IPathProvider _paths;
    private readonly McpConfigMerger _mcpMerger;

    public DeployTarget Target => DeployTarget.CopilotCli;

    public CopilotCliTarget(IFileSystem fs, IPathProvider paths, McpConfigMerger mcpMerger)
    {
        _fs = fs;
        _paths = paths;
        _mcpMerger = mcpMerger;
    }

    public DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options)
    {
        var actions = new List<DeployAction>();
        foreach (var asset in assets.Where(a => a.Type != AssetType.McpConfig))
        {
            var path = ResolveAssetPath(asset, options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(path) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: path,
                SourceAssets: [asset.Name]));
        }
        var mcpAssets = assets.Where(a => a.Type == AssetType.McpConfig).ToList();
        if (mcpAssets.Count > 0)
        {
            var mcpPath = ResolveMcpPath(options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(mcpPath) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: mcpPath,
                SourceAssets: mcpAssets.Select(a => a.Name).ToList()));
        }
        return new DeployPlan(actions);
    }

    public void Apply(DeployPlan plan, IReadOnlyList<AssetDefinition> assets, DeployOptions options)
    {
        var byName = assets.ToDictionary(a => a.Name, StringComparer.Ordinal);

        foreach (var action in plan.Actions)
        {
            if (action.Kind == DeployActionKind.Skip || action.Kind == DeployActionKind.Error)
                continue;

            EnsureDirectory(action.TargetPath);

            var sources = action.SourceAssets.Select(n => byName[n]).ToList();
            if (sources[0].Type == AssetType.McpConfig)
            {
                var merged = _mcpMerger.Merge(sources);
                var json = JsonSerializer.Serialize(
                    new Dictionary<string, object?> { ["servers"] = merged },
                    new JsonSerializerOptions { WriteIndented = true });
                _fs.File.WriteAllText(action.TargetPath, json);
            }
            else
            {
                _fs.File.WriteAllText(action.TargetPath, sources[0].Body);
            }
        }
    }

    private string ResolveAssetPath(AssetDefinition asset, DeployOptions options)
    {
        var root = options.Mode == DeployMode.Repo
            ? _fs.Path.Combine(options.DestinationPath, _paths.GetRepoSubPath(DeployTarget.CopilotCli, asset.Type))
            : _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.CopilotCli), TypeFolder(asset.Type));

        return asset.Type switch
        {
            AssetType.Skill => _fs.Path.Combine(root, asset.Name, "SKILL.md"),
            AssetType.Instruction => _fs.Path.Combine(root, asset.Name + ".md"),
            AssetType.Agent => _fs.Path.Combine(root, asset.Name + ".md"),
            AssetType.McpConfig => _fs.Path.Combine(root, asset.Name + ".md"),
            _ => throw new ArgumentOutOfRangeException(nameof(asset)),
        };
    }

    private string ResolveMcpPath(DeployOptions options)
    {
        if (options.Mode == DeployMode.Repo)
            return _fs.Path.Combine(options.DestinationPath, _paths.GetMcpSettingsRelativePath(DeployTarget.CopilotCli));
        return _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.CopilotCli), "mcp.json");
    }

    private static string TypeFolder(AssetType type) => type switch
    {
        AssetType.Instruction => "instructions",
        AssetType.Skill => "skills",
        AssetType.Agent => "agents",
        AssetType.McpConfig => "",
        _ => throw new ArgumentOutOfRangeException(nameof(type)),
    };

    private void EnsureDirectory(string path)
    {
        var dir = _fs.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !_fs.Directory.Exists(dir))
            _fs.Directory.CreateDirectory(dir);
    }
}
