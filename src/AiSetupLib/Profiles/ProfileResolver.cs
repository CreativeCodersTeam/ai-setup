using System.IO.Abstractions;
using AiSetupLib.Models;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace AiSetupLib.Profiles;

public sealed class ProfileResolver : IProfileResolver
{
    private readonly IFileSystem _fs;
    private readonly IDeserializer _yaml = new DeserializerBuilder()
        .WithNamingConvention(HyphenatedNamingConvention.Instance)
        .IgnoreUnmatchedProperties()
        .Build();

    public ProfileResolver(IFileSystem fs) => _fs = fs;

    public Profile Resolve(string repoRoot, string profileName)
    {
        var path = _fs.Path.Combine(repoRoot, "profiles", profileName + ".yaml");
        if (!_fs.File.Exists(path))
            throw new FileNotFoundException($"Profile '{profileName}' not found at {path}", path);

        var text = _fs.File.ReadAllText(path);
        var dto = _yaml.Deserialize<ProfileDto>(text) ?? new ProfileDto();

        return new Profile(
            Name: dto.Name ?? profileName,
            Description: dto.Description ?? "",
            Agents: dto.Agents ?? [],
            Instructions: dto.Instructions ?? [],
            Skills: dto.Skills ?? [],
            McpConfigs: dto.McpConfigs ?? []);
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
