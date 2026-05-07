using AiSetup.Lib.Discovery;
using AiSetup.Lib.Models;
using FluentAssertions;
using Xunit;

namespace AiSetup.Lib.Tests.Discovery;

public class AssetDiscoveryServiceTests
{
    private const string Repo = "/repo";

    private static (AssetDiscoveryService Service, InMemoryFileSystem Fs) CreateSut()
    {
        var fs = new InMemoryFileSystem();
        var sut = new AssetDiscoveryService(fs, new FrontmatterParser());

        return (sut, fs);
    }

    [Fact]
    public void Discover_FindsInstructionsAgentsAndMcp()
    {
        var (sut, fs) = CreateSut();
        fs.AddFile($"{Repo}/instructions/csharp/csharp.instructions.md", """
                                                                          ---
                                                                          name: csharp
                                                                          description: C# rules
                                                                          ---
                                                                          body
                                                                          """);
        fs.AddFile($"{Repo}/agents/dotnet-developer.md", """
                                                         ---
                                                         name: dotnet-developer
                                                         description: dn dev
                                                         ---
                                                         body
                                                         """);
        fs.AddFile($"{Repo}/mcp-configs/github.yaml", """
                                                      ---
                                                      name: github
                                                      ---
                                                      command: gh
                                                      """);

        var assets = sut.Discover(Repo);

        assets.Should().HaveCount(3);
        assets.Should().Contain(a => a.Type == AssetType.Instruction && a.Name == "csharp");
        assets.Should().Contain(a => a.Type == AssetType.Agent && a.Name == "dotnet-developer");
        assets.Should().Contain(a => a.Type == AssetType.McpConfig && a.Name == "github");
    }

    [Fact]
    public void Discover_DetectsSkillBySkillMd()
    {
        var (sut, fs) = CreateSut();
        fs.AddFile($"{Repo}/skills/csharp/dotnet-tester/SKILL.md", """
                                                                   ---
                                                                   name: dotnet-tester
                                                                   description: tester
                                                                   ---
                                                                   body
                                                                   """);

        var assets = sut.Discover(Repo);

        assets.Should().ContainSingle()
            .Which.Type.Should().Be(AssetType.Skill);
    }

    [Fact]
    public void Discover_CollectsWarningsForInvalidFrontmatter()
    {
        var (sut, fs) = CreateSut();
        fs.AddFile($"{Repo}/agents/broken.md", "---\nbroken yaml");
        var warnings = new List<string>();

        var assets = sut.Discover(Repo, warnings);

        assets.Should().BeEmpty();
        warnings.Should().HaveCount(1);
        warnings[0].Should().Contain("broken.md");
    }
}
