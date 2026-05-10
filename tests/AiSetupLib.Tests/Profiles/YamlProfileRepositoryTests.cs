using AiSetup.Models;
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
    public void Find_WithFullProfileFile_LoadsAllSections()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "dotnet-dev.yaml"),
            "name: dotnet-dev\ndescription: \".NET development\"\nagents:\n  - dotnet-developer\ninstructions:\n  - csharp/csharp.instructions\nskills:\n  - csharp/dotnet-tester\nmcp-configs:\n  - github\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var profile = sut.Find("dotnet-dev");

        // Assert
        profile.Should().NotBeNull();
        profile!.Description.Should().Be(".NET development");
        profile.Agents.Should().ContainSingle().Which.Id.Should().Be("dotnet-developer");
        profile.Instructions.Should().ContainSingle().Which.Id.Should().Be("csharp/csharp.instructions");
        profile.Skills.Should().ContainSingle().Which.Id.Should().Be("csharp/dotnet-tester");
        profile.McpConfigs.Should().ContainSingle().Which.Id.Should().Be("github");
    }

    [Fact]
    public void Find_WithSettingsList_ParsesSettingsRefs()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "dev.yaml"),
            "name: dev\nsettings:\n  - claude-code/base\n  - copilot-cli/defaults@local\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var profile = sut.Find("dev");

        // Assert
        profile.Should().NotBeNull();
        profile!.Settings.Should().HaveCount(2);
        profile.Settings[0].Should().Be(new ProfileAssetRef("claude-code/base", DeployMode.Repo));
        profile.Settings[1].Should().Be(new ProfileAssetRef("copilot-cli/defaults", DeployMode.Local));
    }

    [Fact]
    public void Find_WithBareEntries_DefaultsToRepoMode()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "p.yaml"),
            "name: p\nagents:\n  - a\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var profile = sut.Find("p");

        // Assert
        profile.Should().NotBeNull();
        profile!.Agents.Should().ContainSingle().Which.Should().Be(new ProfileAssetRef("a", DeployMode.Repo));
    }

    [Fact]
    public void Find_WithModeSuffixes_ParsesPerAssetMode()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "p.yaml"),
            "name: p\nagents:\n  - keep@repo\n  - local-agent@local\ninstructions:\n  - csharp/csharp.instructions@LOCAL\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var profile = sut.Find("p");

        // Assert
        profile.Should().NotBeNull();
        profile!.Agents.Should().BeEquivalentTo(new[]
        {
            new ProfileAssetRef("keep", DeployMode.Repo),
            new ProfileAssetRef("local-agent", DeployMode.Local)
        });
        profile.Instructions.Should().ContainSingle()
            .Which.Should().Be(new ProfileAssetRef("csharp/csharp.instructions", DeployMode.Local));
    }

    [Fact]
    public void All_WithInvalidModeSuffix_ProducesWarningAndDefaultsToRepo()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "p.yaml"),
            "name: p\nagents:\n  - a@bogus\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var profile = sut.Find("p");

        // Assert
        profile!.Agents.Should().ContainSingle().Which.Should().Be(new ProfileAssetRef("a", DeployMode.Repo));
        sut.Warnings.Should().Contain(w => w.Contains("invalid mode 'bogus'") && w.Contains("'a'"));
    }

    [Fact]
    public void All_WithEmptyAssetIdInSuffixEntry_ProducesWarningAndSkips()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "p.yaml"),
            "name: p\nagents:\n  - \"@local\"\n  - real-agent\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var profile = sut.Find("p");

        // Assert
        profile!.Agents.Should().ContainSingle().Which.Id.Should().Be("real-agent");
        sut.Warnings.Should().Contain(w => w.Contains("empty asset id"));
    }

    [Fact]
    public void Find_WhenNameMissingInFile_FallsBackToFileStem()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "minimal.yaml"),
            "agents:\n  - a\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var result = sut.Find("minimal");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void Find_WithUnknownProfile_ReturnsNull()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "exists.yaml"),
            "name: exists\nagents:\n  - a\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var result = sut.Find("does-not-exist");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Find_WithDifferentCaseId_IsCaseInsensitive()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "dotnet-dev.yaml"),
            "name: dotnet-dev\nagents:\n  - a\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var result = sut.Find("DOTNET-DEV");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void All_WithMultipleProfiles_ReturnsAllDiscovered()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "p1.yaml"),
            "name: p1\nagents:\n  - a\n");
        File.WriteAllText(Path.Combine(_root, "profiles", "p2.yml"),
            "name: p2\nagents:\n  - b\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var profiles = sut.All();

        // Assert
        profiles.Should().HaveCount(2);
        profiles.Select(p => p.Name).Should().BeEquivalentTo("p1", "p2");
    }

    [Fact]
    public void All_WithNonYamlFiles_IgnoresThem()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "p.yaml"),
            "name: p\nagents:\n  - a\n");
        File.WriteAllText(Path.Combine(_root, "profiles", "readme.md"), "not a profile");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var result = sut.All();

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public void All_WhenProfilesFolderMissing_ReturnsEmpty()
    {
        // Arrange
        var emptyRoot = Path.Combine(Path.GetTempPath(), "ai-setup-empty-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(emptyRoot);
        try
        {
            var sut = new YamlProfileRepository(new FileSystem(), emptyRoot);

            // Act
            var result = sut.All();

            // Assert
            result.Should().BeEmpty();
        }
        finally
        {
            Directory.Delete(emptyRoot, recursive: true);
        }
    }

    [Fact]
    public void All_WithMalformedProfileYaml_ProducesWarningAndContinues()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "bad.yaml"), ":::: not yaml ::::");
        File.WriteAllText(Path.Combine(_root, "profiles", "good.yaml"),
            "name: good\nagents:\n  - x\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        var profiles = sut.All();

        // Assert
        profiles.Should().ContainSingle().Which.Name.Should().Be("good");
        sut.Warnings.Should().Contain(w => w.Contains("profile parse error"));
    }

    [Fact]
    public void All_WithDuplicateProfileName_ProducesWarning()
    {
        // Arrange
        File.WriteAllText(Path.Combine(_root, "profiles", "a.yaml"), "name: same\nagents:\n  - x\n");
        File.WriteAllText(Path.Combine(_root, "profiles", "b.yaml"), "name: same\nagents:\n  - y\n");
        var sut = new YamlProfileRepository(new FileSystem(), _root);

        // Act
        _ = sut.All();

        // Assert
        sut.Warnings.Should().Contain(w => w.Contains("duplicate profile name 'same'"));
    }
}
