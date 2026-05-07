using AiSetup.Lib.Exceptions;
using AiSetup.Lib.IO;
using AiSetup.Lib.Models;
using CreativeCoders.Core;

namespace AiSetup.Lib.Discovery;

/// <inheritdoc cref="IAssetDiscovery"/>
public sealed class AssetDiscoveryService : IAssetDiscovery
{
    private const string AgentsFolder = "agents";
    private const string InstructionsFolder = "instructions";
    private const string SkillsFolder = "skills";
    private const string McpConfigsFolder = "mcp-configs";

    private readonly IFileSystem _fileSystem;
    private readonly IFrontmatterParser _frontmatterParser;

    /// <summary>Initialises a new instance.</summary>
    public AssetDiscoveryService(IFileSystem fileSystem, IFrontmatterParser frontmatterParser)
    {
        _fileSystem = Ensure.NotNull(fileSystem, nameof(fileSystem));
        _frontmatterParser = Ensure.NotNull(frontmatterParser, nameof(frontmatterParser));
    }

    /// <inheritdoc />
    public IReadOnlyList<AssetDefinition> Discover(string repoRoot, IList<string>? warningSink = null)
    {
        Ensure.IsNotNullOrWhitespace(repoRoot, nameof(repoRoot));

        var results = new List<AssetDefinition>();

        DiscoverFlatFolder(repoRoot, AgentsFolder, AssetType.Agent, results, warningSink);
        DiscoverFlatFolder(repoRoot, InstructionsFolder, AssetType.Instruction, results, warningSink);
        DiscoverFlatFolder(repoRoot, McpConfigsFolder, AssetType.McpConfig, results, warningSink);
        DiscoverSkills(repoRoot, results, warningSink);

        return results;
    }

    private void DiscoverFlatFolder(
        string repoRoot,
        string folderName,
        AssetType type,
        List<AssetDefinition> results,
        IList<string>? warningSink)
    {
        var root = Path.Combine(repoRoot, folderName);

        if (!_fileSystem.DirectoryExists(root))
        {
            return;
        }

        var pattern = type == AssetType.McpConfig ? "*.yaml" : "*.md";

        foreach (var file in _fileSystem.EnumerateFiles(root, pattern, recursive: true))
        {
            if (Path.GetFileName(file).Equals("manifest.yaml", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var asset = TryReadAsset(repoRoot, file, type, warningSink);

            if (asset is not null)
            {
                results.Add(asset);
            }
        }
    }

    private void DiscoverSkills(string repoRoot, List<AssetDefinition> results, IList<string>? warningSink)
    {
        var root = Path.Combine(repoRoot, SkillsFolder);

        if (!_fileSystem.DirectoryExists(root))
        {
            return;
        }

        foreach (var skillFile in EnumerateSkillFiles(root))
        {
            var asset = TryReadAsset(repoRoot, skillFile, AssetType.Skill, warningSink);

            if (asset is not null)
            {
                results.Add(asset);
            }
        }
    }

    private IEnumerable<string> EnumerateSkillFiles(string skillsRoot)
    {
        var stack = new Stack<string>();
        stack.Push(skillsRoot);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            var skillMd = Path.Combine(current, "SKILL.md");

            if (_fileSystem.FileExists(skillMd))
            {
                yield return skillMd;

                continue;
            }

            var folderName = Path.GetFileName(current);
            var nameMatched = Path.Combine(current, folderName + ".md");

            if (!current.Equals(skillsRoot, StringComparison.Ordinal) && _fileSystem.FileExists(nameMatched))
            {
                yield return nameMatched;

                continue;
            }

            foreach (var child in _fileSystem.EnumerateDirectories(current))
            {
                stack.Push(child);
            }
        }
    }

    private AssetDefinition? TryReadAsset(
        string repoRoot,
        string absolutePath,
        AssetType type,
        IList<string>? warningSink)
    {
        try
        {
            var content = _fileSystem.ReadAllText(absolutePath);
            var parsed = _frontmatterParser.Parse(content, absolutePath);

            return BuildAsset(repoRoot, absolutePath, type, content, parsed);
        }
        catch (InvalidFrontmatterException ex)
        {
            warningSink?.Add(ex.Message);

            return null;
        }
    }

    private static AssetDefinition BuildAsset(
        string repoRoot,
        string absolutePath,
        AssetType type,
        string rawContent,
        FrontmatterParseResult parsed)
    {
        var fm = parsed.Frontmatter;

        var name = ReadString(fm, "name") ?? DeriveDefaultName(absolutePath, type);
        var description = ReadString(fm, "description") ?? string.Empty;
        var tags = ReadStringList(fm, "tags");
        var targets = ReadTargets(fm);
        var applyTo = ReadString(fm, "applyTo");
        var relative = Path.GetRelativePath(repoRoot, absolutePath);

        return new AssetDefinition(
            Name: name,
            Description: description,
            Type: type,
            Tags: tags,
            Targets: targets,
            ApplyTo: applyTo,
            RelativePath: relative,
            AbsolutePath: absolutePath,
            RawContent: rawContent,
            Body: parsed.Body,
            Frontmatter: fm);
    }

    private static string DeriveDefaultName(string absolutePath, AssetType type)
    {
        if (type == AssetType.Skill)
        {
            var dir = Path.GetDirectoryName(absolutePath);

            if (!string.IsNullOrEmpty(dir))
            {
                return Path.GetFileName(dir);
            }
        }

        var fileName = Path.GetFileNameWithoutExtension(absolutePath);

        if (fileName.EndsWith(".instructions", StringComparison.OrdinalIgnoreCase))
        {
            fileName = fileName[..^".instructions".Length];
        }

        return fileName;
    }

    private static string? ReadString(IReadOnlyDictionary<string, object?> fm, string key)
    {
        if (!fm.TryGetValue(key, out var value) || value is null)
        {
            return null;
        }

        return value.ToString();
    }

    private static IReadOnlyList<string> ReadStringList(IReadOnlyDictionary<string, object?> fm, string key)
    {
        if (!fm.TryGetValue(key, out var value) || value is null)
        {
            return Array.Empty<string>();
        }

        if (value is IEnumerable<object?> list)
        {
            return list.Where(x => x is not null)
                .Select(x => x!.ToString()!)
                .ToArray();
        }

        return new[] { value.ToString()! };
    }

    private static IReadOnlyList<DeployTarget> ReadTargets(IReadOnlyDictionary<string, object?> fm)
    {
        var rawTargets = ReadStringList(fm, "targets");

        if (rawTargets.Count == 0)
        {
            return Array.Empty<DeployTarget>();
        }

        var result = new List<DeployTarget>(rawTargets.Count);

        foreach (var raw in rawTargets)
        {
            var target = ParseTarget(raw);

            if (target.HasValue)
            {
                result.Add(target.Value);
            }
        }

        return result;
    }

    private static DeployTarget? ParseTarget(string raw)
    {
        return raw.Replace("-", "", StringComparison.Ordinal).ToLowerInvariant() switch
        {
            "copilotcli" => DeployTarget.CopilotCli,
            "claudecode" => DeployTarget.ClaudeCode,
            _ => null,
        };
    }
}
