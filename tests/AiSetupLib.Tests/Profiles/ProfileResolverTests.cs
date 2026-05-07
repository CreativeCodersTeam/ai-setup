using AiSetup.Lib.Profiles;
using FluentAssertions;
using Xunit;

namespace AiSetup.Lib.Tests.Profiles;

public class ProfileResolverTests
{
    private const string Repo = "/repo";

    [Fact]
    public void LoadProfile_ParsesAllSections()
    {
        var fs = new InMemoryFileSystem();
        fs.AddFile($"{Repo}/profiles/dotnet-dev.yaml", """
                                                       name: dotnet-dev
                                                       description: ".NET dev"
                                                       agents:
                                                         - dotnet-developer
                                                       instructions:
                                                         - csharp/csharp.instructions
                                                       skills:
                                                         - csharp/dotnet-tester
                                                       mcp-configs:
                                                         - github
                                                       """);

        var profile = new ProfileResolver(fs).LoadProfile(Repo, "dotnet-dev");

        profile.Should().NotBeNull();
        profile!.Name.Should().Be("dotnet-dev");
        profile.Agents.Should().ContainSingle().Which.Should().Be("dotnet-developer");
        profile.Skills.Should().ContainSingle().Which.Should().Be("csharp/dotnet-tester");
        profile.McpConfigs.Should().ContainSingle().Which.Should().Be("github");
    }

    [Fact]
    public void LoadProfile_NotFound_ReturnsNull()
    {
        var fs = new InMemoryFileSystem();
        var resolver = new ProfileResolver(fs);

        resolver.LoadProfile(Repo, "missing").Should().BeNull();
    }

    [Fact]
    public void ListProfiles_ReturnsAllInFolder()
    {
        var fs = new InMemoryFileSystem();
        fs.AddFile($"{Repo}/profiles/a.yaml", "name: a\n");
        fs.AddFile($"{Repo}/profiles/b.yaml", "name: b\n");

        var profiles = new ProfileResolver(fs).ListProfiles(Repo);

        profiles.Should().HaveCount(2);
        profiles.Select(p => p.Name).Should().BeEquivalentTo(new[] { "a", "b" });
    }
}
