using AiSetup.Models;
using AiSetup.Platform;
using CreativeCoders.Core;
using YamlDotNet.Serialization;

namespace AiSetup.Discovery;

/// <summary>
/// Discovers assets by scanning the layout described in the design document
/// (agents/, instructions/, skills/, mcp-configs/) under a source repository root.
/// </summary>
public sealed class FileSystemAssetRepository : IAssetRepository
{
    private const string AgentsFolder = "agents";
    private const string InstructionsFolder = "instructions";
    private const string SkillsFolder = "skills";
    private const string McpConfigsFolder = "mcp-configs";
    private const string SkillEntryFile = "SKILL.md";

    private static readonly IDeserializer YamlDeserializer = new DeserializerBuilder().Build();

    private readonly IFileSystem _fileSystem;
    private readonly string _repoRoot;
    private readonly Dictionary<(AssetType Type, string Id), AssetDefinition> _index = new();
    private readonly List<string> _warnings = [];
    private bool _loaded;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="fileSystem">File system abstraction.</param>
    /// <param name="repoRoot">Absolute path of the ai-setup source repository.</param>
    public FileSystemAssetRepository(IFileSystem fileSystem, string repoRoot)
    {
        _fileSystem = Ensure.NotNull(fileSystem);
        _repoRoot = Ensure.IsNotNullOrWhitespace(repoRoot);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> Warnings
    {
        get
        {
            EnsureLoaded();
            return _warnings;
        }
    }

    /// <inheritdoc />
    public AssetDefinition? Find(AssetType type, string id)
    {
        Ensure.IsNotNullOrWhitespace(id);
        EnsureLoaded();
        return _index.TryGetValue((type, NormalizeId(id)), out var asset) ? asset : null;
    }

    /// <inheritdoc />
    public IReadOnlyList<AssetDefinition> All(AssetType? type = null)
    {
        EnsureLoaded();

        if (type is null)
        {
            return _index.Values.ToArray();
        }

        return _index.Values.Where(a => a.Type == type.Value).ToArray();
    }

    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        LoadInstructions();
        LoadAgents();
        LoadSkills();
        LoadMcpConfigs();
    }

    private void LoadInstructions()
    {
        LoadMarkdownAssets(InstructionsFolder, AssetType.Instruction);
    }

    private void LoadAgents()
    {
        LoadMarkdownAssets(AgentsFolder, AssetType.Agent);
    }

    private void LoadMarkdownAssets(string subFolder, AssetType type)
    {
        var root = Path.Combine(_repoRoot, subFolder);

        if (!_fileSystem.DirectoryExists(root))
        {
            return;
        }

        foreach (var relative in _fileSystem.EnumerateFilesRecursive(root))
        {
            if (!relative.EndsWith(".md", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var fullPath = Path.Combine(root, relative);
            var content = _fileSystem.ReadAllText(fullPath);
            var parsed = FrontmatterParser.Parse(content);

            if (parsed.Warning is not null)
            {
                _warnings.Add($"{fullPath}: {parsed.Warning}");
            }

            var id = NormalizeId(StripExtension(relative));
            var asset = new AssetDefinition(
                Id: id,
                Type: type,
                Name: parsed.Values.GetString("name") ?? Path.GetFileNameWithoutExtension(relative),
                Description: parsed.Values.GetString("description") ?? string.Empty,
                Tags: parsed.Values.GetStringList("tags"),
                Targets: parsed.Values.GetTargets(),
                SourcePath: fullPath,
                ApplyTo: parsed.Values.GetString("applyTo"),
                Frontmatter: parsed.Values,
                Body: parsed.Body);

            _index[(type, id)] = asset;
        }
    }

    private void LoadSkills()
    {
        var root = Path.Combine(_repoRoot, SkillsFolder);

        if (!_fileSystem.DirectoryExists(root))
        {
            return;
        }

        foreach (var relative in _fileSystem.EnumerateFilesRecursive(root))
        {
            if (!string.Equals(Path.GetFileName(relative), SkillEntryFile, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var skillFile = Path.Combine(root, relative);
            var folderRelative = Path.GetDirectoryName(relative) ?? string.Empty;
            var folder = Path.Combine(root, folderRelative);
            var content = _fileSystem.ReadAllText(skillFile);
            var parsed = FrontmatterParser.Parse(content);

            if (parsed.Warning is not null)
            {
                _warnings.Add($"{skillFile}: {parsed.Warning}");
            }

            var id = NormalizeId(folderRelative);
            var files = _fileSystem.EnumerateFilesRecursive(folder);

            var skill = new SkillAsset(
                Id: id,
                Name: parsed.Values.GetString("name") ?? Path.GetFileName(folderRelative),
                Description: parsed.Values.GetString("description") ?? string.Empty,
                Tags: parsed.Values.GetStringList("tags"),
                Targets: parsed.Values.GetTargets(),
                SourcePath: skillFile,
                ApplyTo: parsed.Values.GetString("applyTo"),
                Frontmatter: parsed.Values,
                Body: parsed.Body,
                Folder: folder,
                Files: files);

            _index[(AssetType.Skill, id)] = skill;
        }
    }

    private void LoadMcpConfigs()
    {
        var root = Path.Combine(_repoRoot, McpConfigsFolder);

        if (!_fileSystem.DirectoryExists(root))
        {
            return;
        }

        foreach (var relative in _fileSystem.EnumerateFilesRecursive(root))
        {
            if (!IsYamlFile(relative))
            {
                continue;
            }

            var fullPath = Path.Combine(root, relative);
            var content = _fileSystem.ReadAllText(fullPath);
            IReadOnlyDictionary<string, object?> values;

            try
            {
                var raw = YamlDeserializer.Deserialize<Dictionary<object, object?>>(content);
                values = raw is null
                    ? new Dictionary<string, object?>()
                    : raw.ToDictionary(
                        kvp => kvp.Key.ToString() ?? string.Empty,
                        kvp => kvp.Value);
            }
            catch (Exception ex)
            {
                _warnings.Add($"{fullPath}: MCP config parse error: {ex.Message}");
                continue;
            }

            var id = NormalizeId(StripExtension(relative));
            var asset = new AssetDefinition(
                Id: id,
                Type: AssetType.McpConfig,
                Name: values.GetString("name") ?? Path.GetFileNameWithoutExtension(relative),
                Description: values.GetString("description") ?? string.Empty,
                Tags: values.GetStringList("tags"),
                Targets: values.GetTargets(),
                SourcePath: fullPath,
                ApplyTo: null,
                Frontmatter: values,
                Body: content);

            _index[(AssetType.McpConfig, id)] = asset;
        }
    }

    private static bool IsYamlFile(string path)
    {
        return path.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
            || path.EndsWith(".yml", StringComparison.OrdinalIgnoreCase);
    }

    private static string StripExtension(string relative)
    {
        var dir = Path.GetDirectoryName(relative);
        var stem = Path.GetFileNameWithoutExtension(relative);
        return string.IsNullOrEmpty(dir) ? stem : Path.Combine(dir, stem);
    }

    private static string NormalizeId(string id)
    {
        return id.Replace('\\', '/');
    }
}
