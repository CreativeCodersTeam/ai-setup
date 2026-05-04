using System.IO.Abstractions;
using AiSetupLib.Models;

namespace AiSetupLib.Discovery;

public sealed class AssetDiscoveryService : IAssetDiscovery
{
    private static readonly Dictionary<string, AssetType> TypeDirs = new(StringComparer.Ordinal)
    {
        ["instructions"] = AssetType.Instruction,
        ["agents"] = AssetType.Agent,
        ["skills"] = AssetType.Skill,
        ["mcp-configs"] = AssetType.McpConfig,
    };

    private static readonly Dictionary<string, AssetType> FrontmatterTypes = new(StringComparer.Ordinal)
    {
        ["instruction"] = AssetType.Instruction,
        ["agent"] = AssetType.Agent,
        ["skill"] = AssetType.Skill,
        ["mcp-config"] = AssetType.McpConfig,
    };

    private static readonly Dictionary<string, DeployTarget> TargetMap = new(StringComparer.Ordinal)
    {
        ["copilot-cli"] = DeployTarget.CopilotCli,
        ["claude-code"] = DeployTarget.ClaudeCode,
    };

    private readonly IFileSystem _fs;
    private readonly FrontmatterParser _parser;

    public AssetDiscoveryService(IFileSystem fs, FrontmatterParser parser)
    {
        _fs = fs;
        _parser = parser;
    }

    public IReadOnlyList<AssetDefinition> Discover(string repoRoot)
    {
        var results = new List<AssetDefinition>();
        foreach (var (dir, defaultType) in TypeDirs)
        {
            var typeRoot = _fs.Path.Combine(repoRoot, dir);
            if (!_fs.Directory.Exists(typeRoot))
                continue;

            foreach (var file in EnumerateAssetFiles(typeRoot, defaultType))
            {
                var asset = TryReadAsset(file, typeRoot, defaultType);
                if (asset is not null)
                    results.Add(asset);
            }
        }
        return results;
    }

    private IEnumerable<string> EnumerateAssetFiles(string typeRoot, AssetType type)
    {
        if (type == AssetType.Skill)
        {
            return _fs.Directory.EnumerateFiles(typeRoot, "SKILL.md", SearchOption.AllDirectories);
        }
        if (type == AssetType.McpConfig)
        {
            return _fs.Directory.EnumerateFiles(typeRoot, "*.yaml", SearchOption.AllDirectories)
                .Concat(_fs.Directory.EnumerateFiles(typeRoot, "*.yml", SearchOption.AllDirectories));
        }
        return _fs.Directory.EnumerateFiles(typeRoot, "*.md", SearchOption.AllDirectories);
    }

    private AssetDefinition? TryReadAsset(string path, string typeRoot, AssetType defaultType)
    {
        var raw = _fs.File.ReadAllText(path);
        var fm = _parser.Parse(raw);
        if (!fm.HasFrontmatter)
            return null;

        var name = ComputeName(path, typeRoot, defaultType);
        var description = AsString(fm.Frontmatter, "description") ?? "";
        var type = AsString(fm.Frontmatter, "type") is { } t && FrontmatterTypes.TryGetValue(t, out var parsed)
            ? parsed
            : defaultType;
        var tags = AsStringList(fm.Frontmatter, "tags");
        var targets = AsStringList(fm.Frontmatter, "targets")
            .Where(s => TargetMap.ContainsKey(s))
            .Select(s => TargetMap[s])
            .ToList()
            .AsReadOnly();
        var applyTo = AsString(fm.Frontmatter, "applyTo");

        return new AssetDefinition(
            Name: name,
            Description: description,
            Type: type,
            Tags: tags,
            Targets: targets,
            ApplyTo: applyTo,
            SourcePath: path,
            Body: fm.Body);
    }

    private string ComputeName(string path, string typeRoot, AssetType type)
    {
        if (type == AssetType.Skill)
        {
            var folder = _fs.Path.GetDirectoryName(path)!;
            return Relative(folder, typeRoot);
        }
        var rel = Relative(path, typeRoot);
        var dot = rel.LastIndexOf('.');
        return dot > 0 ? rel[..dot] : rel;
    }

    private string Relative(string path, string root)
    {
        var rel = _fs.Path.GetRelativePath(root, path);
        return rel.Replace('\\', '/');
    }

    private static string? AsString(IReadOnlyDictionary<string, object?> dict, string key)
        => dict.TryGetValue(key, out var v) ? v as string : null;

    private static IReadOnlyList<string> AsStringList(IReadOnlyDictionary<string, object?> dict, string key)
    {
        if (!dict.TryGetValue(key, out var v)) return [];
        return v switch
        {
            IReadOnlyList<string> list => list,
            string s => [s],
            _ => [],
        };
    }
}
