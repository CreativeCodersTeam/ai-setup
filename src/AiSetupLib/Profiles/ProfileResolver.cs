using AiSetup.Lib.IO;
using AiSetup.Lib.Models;
using CreativeCoders.Core;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AiSetup.Lib.Profiles;

/// <inheritdoc cref="IProfileResolver"/>
public sealed class ProfileResolver : IProfileResolver
{
    private const string ProfilesFolder = "profiles";

    private readonly IFileSystem _fileSystem;
    private readonly IDeserializer _deserializer;

    /// <summary>Initialises a new instance.</summary>
    public ProfileResolver(IFileSystem fileSystem)
    {
        _fileSystem = Ensure.NotNull(fileSystem, nameof(fileSystem));
        _deserializer = new DeserializerBuilder()
            .WithNamingConvention(HyphenatedNamingConvention.Instance)
            .IgnoreUnmatchedProperties()
            .Build();
    }

    /// <inheritdoc />
    public IReadOnlyList<Profile> ListProfiles(string repoRoot)
    {
        Ensure.IsNotNullOrWhitespace(repoRoot, nameof(repoRoot));

        var folder = Path.Combine(repoRoot, ProfilesFolder);
        var files = _fileSystem.EnumerateFiles(folder, "*.yaml", recursive: false);
        var result = new List<Profile>(files.Count);

        foreach (var file in files)
        {
            var profile = LoadFromFile(file);

            if (profile is not null)
            {
                result.Add(profile);
            }
        }

        return result;
    }

    /// <inheritdoc />
    public Profile? LoadProfile(string repoRoot, string name)
    {
        Ensure.IsNotNullOrWhitespace(repoRoot, nameof(repoRoot));
        Ensure.IsNotNullOrWhitespace(name, nameof(name));

        var path = Path.Combine(repoRoot, ProfilesFolder, $"{name}.yaml");

        if (!_fileSystem.FileExists(path))
        {
            return null;
        }

        return LoadFromFile(path);
    }

    private Profile? LoadFromFile(string path)
    {
        var raw = _fileSystem.ReadAllText(path);
        var dto = _deserializer.Deserialize<ProfileDto?>(raw);

        if (dto is null)
        {
            return null;
        }

        var name = !string.IsNullOrWhiteSpace(dto.Name)
            ? dto.Name!
            : Path.GetFileNameWithoutExtension(path);

        return new Profile(
            Name: name,
            Description: dto.Description,
            Agents: (IReadOnlyList<string>?)dto.Agents ?? Array.Empty<string>(),
            Instructions: (IReadOnlyList<string>?)dto.Instructions ?? Array.Empty<string>(),
            Skills: (IReadOnlyList<string>?)dto.Skills ?? Array.Empty<string>(),
            McpConfigs: (IReadOnlyList<string>?)dto.McpConfigs ?? Array.Empty<string>());
    }

    private sealed class ProfileDto
    {
        public string? Name { get; set; }

        public string? Description { get; set; }

        public List<string>? Agents { get; set; }

        public List<string>? Instructions { get; set; }

        public List<string>? Skills { get; set; }

        public List<string>? McpConfigs { get; set; }
    }
}
