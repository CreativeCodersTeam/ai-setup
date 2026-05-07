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
}
