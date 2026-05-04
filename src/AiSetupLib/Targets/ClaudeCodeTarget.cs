using System.IO.Abstractions;
using System.Text.Json;
using System.Text.Json.Nodes;
using AiSetupLib.Aggregation;
using AiSetupLib.Models;
using AiSetupLib.Paths;

namespace AiSetupLib.Targets;

public sealed class ClaudeCodeTarget : IDeployTarget
{
    private static readonly JsonSerializerOptions IndentedJson = new() { WriteIndented = true };

    private readonly IFileSystem _fs;
    private readonly IPathProvider _paths;
    private readonly IContentAggregator _aggregator;
    private readonly McpConfigMerger _mcpMerger;

    public DeployTarget Target => DeployTarget.ClaudeCode;

    public ClaudeCodeTarget(
        IFileSystem fs,
        IPathProvider paths,
        IContentAggregator aggregator,
        McpConfigMerger mcpMerger)
    {
        _fs = fs;
        _paths = paths;
        _aggregator = aggregator;
        _mcpMerger = mcpMerger;
    }

    public DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options)
    {
        var actions = new List<DeployAction>();

        var aggregated = assets
            .Where(a => a.Type == AssetType.Instruction || a.Type == AssetType.Agent)
            .ToList();
        if (aggregated.Count > 0)
        {
            var path = ResolveClaudeMdPath(options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(path) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: path,
                SourceAssets: aggregated.Select(a => a.Name).ToList()));
        }

        foreach (var skill in assets.Where(a => a.Type == AssetType.Skill))
        {
            var path = ResolveSkillPath(skill, options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(path) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: path,
                SourceAssets: [skill.Name]));
        }

        var mcps = assets.Where(a => a.Type == AssetType.McpConfig).ToList();
        if (mcps.Count > 0)
        {
            var path = ResolveSettingsPath(options);
            actions.Add(new DeployAction(
                Kind: _fs.File.Exists(path) ? DeployActionKind.Overwrite : DeployActionKind.Create,
                TargetPath: path,
                SourceAssets: mcps.Select(a => a.Name).ToList()));
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
                WriteMergedSettings(action.TargetPath, sources);
            }
            else if (sources[0].Type == AssetType.Skill)
            {
                _fs.File.WriteAllText(action.TargetPath, sources[0].Body);
            }
            else
            {
                var aggregated = _aggregator.Aggregate(sources);
                _fs.File.WriteAllText(action.TargetPath, aggregated);
            }
        }
    }

    private void WriteMergedSettings(string path, IReadOnlyList<AssetDefinition> mcps)
    {
        JsonObject existing;
        if (_fs.File.Exists(path))
        {
            var parsed = JsonNode.Parse(_fs.File.ReadAllText(path));
            existing = parsed as JsonObject ?? new JsonObject();
        }
        else
        {
            existing = new JsonObject();
        }

        var merged = _mcpMerger.Merge(mcps);
        var serversNode = JsonSerializer.SerializeToNode(merged);
        existing["mcpServers"] = serversNode;

        _fs.File.WriteAllText(path, existing.ToJsonString(IndentedJson));
    }

    private string ResolveClaudeMdPath(DeployOptions options)
    {
        var fileName = _paths.GetClaudeAggregatedFileName();
        return options.Mode == DeployMode.Repo
            ? _fs.Path.Combine(options.DestinationPath, fileName)
            : _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.ClaudeCode), fileName);
    }

    private string ResolveSkillPath(AssetDefinition skill, DeployOptions options)
    {
        if (options.Mode == DeployMode.Repo)
        {
            var sub = _paths.GetRepoSubPath(DeployTarget.ClaudeCode, AssetType.Skill);
            return _fs.Path.Combine(options.DestinationPath, sub, skill.Name, "SKILL.md");
        }
        return _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.ClaudeCode), "commands", skill.Name + ".md");
    }

    private string ResolveSettingsPath(DeployOptions options)
    {
        return options.Mode == DeployMode.Repo
            ? _fs.Path.Combine(options.DestinationPath, _paths.GetMcpSettingsRelativePath(DeployTarget.ClaudeCode))
            : _fs.Path.Combine(_paths.GetLocalRoot(DeployTarget.ClaudeCode), "settings.json");
    }

    private void EnsureDirectory(string path)
    {
        var dir = _fs.Path.GetDirectoryName(path);
        if (!string.IsNullOrEmpty(dir) && !_fs.Directory.Exists(dir))
            _fs.Directory.CreateDirectory(dir);
    }
}
