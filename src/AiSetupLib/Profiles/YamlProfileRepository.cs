using AiSetup.Models;
using AiSetup.Platform;
using CreativeCoders.Core;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace AiSetup.Profiles;

/// <summary>
/// Loads profiles from <c>profiles/*.yaml</c> under a repository root.
/// </summary>
public sealed class YamlProfileRepository : IProfileRepository
{
    private const string ProfilesFolder = "profiles";
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().Build();

    private readonly IFileSystem _fileSystem;
    private readonly string _repoRoot;
    private readonly Dictionary<string, Profile> _index = new(StringComparer.OrdinalIgnoreCase);
    private readonly List<string> _warnings = [];
    private bool _loaded;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="fileSystem">File system abstraction.</param>
    /// <param name="repoRoot">Absolute path of the ai-setup source repository.</param>
    public YamlProfileRepository(IFileSystem fileSystem, string repoRoot)
    {
        _fileSystem = Ensure.NotNull(fileSystem);
        _repoRoot = Ensure.IsNotNullOrWhitespace(repoRoot);
    }

    /// <inheritdoc />
    public Profile? Find(string name)
    {
        Ensure.IsNotNullOrWhitespace(name);
        EnsureLoaded();
        return _index.TryGetValue(name, out var profile) ? profile : null;
    }

    /// <inheritdoc />
    public IReadOnlyList<Profile> All()
    {
        EnsureLoaded();
        return _index.Values.ToArray();
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

    private void EnsureLoaded()
    {
        if (_loaded)
        {
            return;
        }

        _loaded = true;
        var root = Path.Combine(_repoRoot, ProfilesFolder);

        if (!_fileSystem.DirectoryExists(root))
        {
            return;
        }

        foreach (var relative in _fileSystem.EnumerateFilesRecursive(root))
        {
            if (!relative.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase)
                && !relative.EndsWith(".yml", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var fullPath = Path.Combine(root, relative);
            var content = _fileSystem.ReadAllText(fullPath);
            Dictionary<object, object?>? raw;

            try
            {
                raw = Deserializer.Deserialize<Dictionary<object, object?>>(content);
            }
            catch (YamlException ex)
            {
                _warnings.Add($"{fullPath}: profile parse error: {ex.Message}");
                continue;
            }

            raw ??= new Dictionary<object, object?>();

            var name = raw.TryGetValue("name", out var nameValue) && nameValue is not null
                ? nameValue.ToString()!
                : Path.GetFileNameWithoutExtension(relative);

            var profile = new Profile(
                Name: name,
                Description: raw.TryGetValue("description", out var desc) ? desc?.ToString() : null,
                Agents: ExtractRefs(raw, "agents", fullPath),
                Instructions: ExtractRefs(raw, "instructions", fullPath),
                Skills: ExtractRefs(raw, "skills", fullPath),
                McpConfigs: ExtractRefs(raw, "mcp-configs", fullPath));

            if (_index.ContainsKey(profile.Name))
            {
                _warnings.Add(
                    $"{fullPath}: duplicate profile name '{profile.Name}'; previous entry overwritten.");
            }

            _index[profile.Name] = profile;
        }
    }

    private IReadOnlyList<ProfileAssetRef> ExtractRefs(IDictionary<object, object?> raw, string key, string filePath)
    {
        if (!raw.TryGetValue(key, out var value) || value is null)
        {
            return [];
        }

        if (value is not IEnumerable<object?> list)
        {
            return [];
        }

        var result = new List<ProfileAssetRef>();

        foreach (var item in list)
        {
            if (item is null)
            {
                continue;
            }

            var reference = ParseRef(item.ToString()!, key, filePath);

            if (reference is not null)
            {
                result.Add(reference);
            }
        }

        return result;
    }

    private ProfileAssetRef? ParseRef(string entry, string key, string filePath)
    {
        var at = entry.LastIndexOf('@');

        if (at < 0)
        {
            return new ProfileAssetRef(entry, DeployMode.Repo);
        }

        var id = entry[..at];
        var modeText = entry[(at + 1)..].Trim().ToLowerInvariant();

        if (string.IsNullOrWhiteSpace(id))
        {
            _warnings.Add($"{filePath}: empty asset id in '{key}' entry '{entry}'; entry ignored.");
            return null;
        }

        switch (modeText)
        {
            case "repo":
                return new ProfileAssetRef(id, DeployMode.Repo);
            case "local":
                return new ProfileAssetRef(id, DeployMode.Local);
            default:
                _warnings.Add(
                    $"{filePath}: invalid mode '{modeText}' for '{id}' in '{key}'; defaulting to 'repo'.");
                return new ProfileAssetRef(id, DeployMode.Repo);
        }
    }
}
