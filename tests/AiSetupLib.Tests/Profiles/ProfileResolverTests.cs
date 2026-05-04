using System.IO.Abstractions.TestingHelpers;
using AiSetupLib.Profiles;

namespace AiSetupLib.Tests.Profiles;

public class ProfileResolverTests
{
    private const string RepoRoot = "/repo";

    [Fact]
    public void Loads_profile_yaml_and_returns_profile_record()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/profiles/dotnet-dev.yaml", new MockFileData("""
            name: dotnet-dev
            description: ".NET assets"
            agents:
              - dotnet-developer
            instructions:
              - csharp/csharp.instructions
            skills:
              - csharp/dotnet-tester
              - csharp/ef-core
            mcp-configs:
              - github
            """));

        var resolver = new ProfileResolver(fs);
        var profile = resolver.Resolve(RepoRoot, "dotnet-dev");

        profile.Name.Should().Be("dotnet-dev");
        profile.Description.Should().Be(".NET assets");
        profile.Agents.Should().ContainSingle("dotnet-developer");
        profile.Instructions.Should().ContainSingle("csharp/csharp.instructions");
        profile.Skills.Should().HaveCount(2);
        profile.McpConfigs.Should().ContainSingle("github");
    }

    [Fact]
    public void Throws_FileNotFoundException_when_profile_missing()
    {
        var fs = new MockFileSystem();
        var resolver = new ProfileResolver(fs);

        var act = () => resolver.Resolve(RepoRoot, "nope");

        act.Should().Throw<FileNotFoundException>()
            .WithMessage("*nope*");
    }

    [Fact]
    public void Defaults_lists_to_empty_when_keys_absent()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/profiles/minimal.yaml", new MockFileData("""
            name: minimal
            description: "x"
            """));

        var profile = new ProfileResolver(fs).Resolve(RepoRoot, "minimal");

        profile.Agents.Should().BeEmpty();
        profile.Skills.Should().BeEmpty();
        profile.Instructions.Should().BeEmpty();
        profile.McpConfigs.Should().BeEmpty();
    }
}
