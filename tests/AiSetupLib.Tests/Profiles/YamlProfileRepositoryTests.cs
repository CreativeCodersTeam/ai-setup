using AiSetup.Platform;
using AiSetup.Profiles;

namespace AiSetup.Tests.Profiles;

public sealed class YamlProfileRepositoryTests : IDisposable
{
    private readonly string _root;

    public YamlProfileRepositoryTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ai-setup-profile-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(Path.Combine(_root, "profiles"));
    }

    

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void Find_LoadsProfileWithAllSections()
    {
        File.WriteAllText(Path.Combine(_root, "profiles", "dotnet-dev.yaml"),
            "name: dotnet-dev\ndescription: \".NET development\"\nagents:\n  - dotnet-developer\ninstructions:\n  - csharp/csharp.instructions\nskills:\n  - csharp/dotnet-tester\nmcp-configs:\n  - github\n");

        var sut = new YamlProfileRepository(new FileSystem(), _root);

        var profile = sut.Find("dotnet-dev");

        profile.Should().NotBeNull();
        profile!.Description.Should().Be(".NET development");
        profile.Agents.Should().ContainSingle().Which.Should().Be("dotnet-developer");
        profile.Instructions.Should().ContainSingle().Which.Should().Be("csharp/csharp.instructions");
        profile.Skills.Should().ContainSingle().Which.Should().Be("csharp/dotnet-tester");
        profile.McpConfigs.Should().ContainSingle().Which.Should().Be("github");
    }

    [Fact]
    public void Find_NameMissingInFile_FallsBackToFileStem()
    {
        File.WriteAllText(Path.Combine(_root, "profiles", "minimal.yaml"),
            "agents:\n  - a\n");

        var sut = new YamlProfileRepository(new FileSystem(), _root);

        sut.Find("minimal").Should().NotBeNull();
    }


    [Fact]
    public void Find_UnknownProfile_ReturnsNull()
    {
        File.WriteAllText(Path.Combine(_root, "profiles", "exists.yaml"),
            "name: exists\nagents:\n  - a\n");

        var sut = new YamlProfileRepository(new FileSystem(), _root);

        sut.Find("does-not-exist").Should().BeNull();
    }

    [Fact]
    public void Find_IsCaseInsensitive()
    {
        File.WriteAllText(Path.Combine(_root, "profiles", "dotnet-dev.yaml"),
            "name: dotnet-dev\nagents:\n  - a\n");

        var sut = new YamlProfileRepository(new FileSystem(), _root);

        sut.Find("DOTNET-DEV").Should().NotBeNull();
    }

    [Fact]
    public void All_ReturnsAllDiscoveredProfiles()
    {
        File.WriteAllText(Path.Combine(_root, "profiles", "p1.yaml"),
            "name: p1\nagents:\n  - a\n");
        File.WriteAllText(Path.Combine(_root, "profiles", "p2.yml"),
            "name: p2\nagents:\n  - b\n");

        var sut = new YamlProfileRepository(new FileSystem(), _root);

        sut.All().Should().HaveCount(2);
        sut.All().Select(p => p.Name).Should().BeEquivalentTo(new[] { "p1", "p2" });
    }

    [Fact]
    public void All_IgnoresNonYamlFiles()
    {
        File.WriteAllText(Path.Combine(_root, "profiles", "p.yaml"),
            "name: p\nagents:\n  - a\n");
        File.WriteAllText(Path.Combine(_root, "profiles", "readme.md"), "not a profile");

        var sut = new YamlProfileRepository(new FileSystem(), _root);

        sut.All().Should().ContainSingle();
    }

    [Fact]
    public void All_MissingProfilesFolder_ReturnsEmpty()
    {
        var emptyRoot = Path.Combine(Path.GetTempPath(), "ai-setup-empty-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(emptyRoot);
        try
        {
            var sut = new YamlProfileRepository(new FileSystem(), emptyRoot);

            sut.All().Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(emptyRoot, recursive: true);
        }
    }
}
