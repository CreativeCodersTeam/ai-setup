using AiSetup.Discovery;
using AiSetup.Models;
using AiSetup.Platform;

namespace AiSetup.Tests.Discovery;

public sealed class FileSystemAssetRepositoryTests : IDisposable
{
    private readonly string _root;

    public FileSystemAssetRepositoryTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ai-setup-repo-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void Find_WithNestedInstructionPath_DiscoversInstruction()
    {
        // Arrange
        WriteFile("instructions/csharp/csharp.instructions.md",
            "---\nname: csharp\ndescription: C#\ntargets: [copilot-cli]\n---\nBody");
        var sut = NewSut();

        // Act
        var asset = sut.Find(AssetType.Instruction, "csharp/csharp.instructions");

        // Assert
        asset.Should().NotBeNull();
        asset!.Name.Should().Be("csharp");
        asset.Targets.Should().ContainSingle().Which.Should().Be(DeployTarget.CopilotCli);
        asset.Type.Should().Be(AssetType.Instruction);
    }

    [Fact]
    public void Find_WithAgentAtRoot_DiscoversAgent()
    {
        // Arrange
        WriteFile("agents/dotnet-developer.md",
            "---\nname: dotnet-developer\ndescription: builds dotnet\n---\nBody");
        var sut = NewSut();

        // Act
        var asset = sut.Find(AssetType.Agent, "dotnet-developer");

        // Assert
        asset.Should().NotBeNull();
    }

    [Fact]
    public void Find_WithSkillFolder_DiscoversSkillWithAllFiles()
    {
        // Arrange
        WriteFile("skills/csharp/dotnet-tester/SKILL.md",
            "---\nname: dotnet-tester\ndescription: \"unit tests\"\n---\nBody");
        WriteFile("skills/csharp/dotnet-tester/references/xunit.md", "ref");
        var sut = NewSut();

        // Act
        var asset = sut.Find(AssetType.Skill, "csharp/dotnet-tester");

        // Assert
        asset.Should().BeOfType<SkillAsset>();
        var skill = (SkillAsset)asset!;
        skill.Files.Should().Contain("SKILL.md");
        skill.Files.Should().Contain(s => s.EndsWith("xunit.md"));
    }

    [Fact]
    public void Find_WithMcpConfig_DiscoversMcpConfig()
    {
        // Arrange
        WriteFile("mcp-configs/github.yaml",
            "name: github\ntype: stdio\ncommand: npx\nargs:\n  - \"-y\"\n  - server\n");
        var sut = NewSut();

        // Act
        var asset = sut.Find(AssetType.McpConfig, "github");

        // Assert
        asset.Should().NotBeNull();
    }

    [Fact]
    public void All_FilteredByType_ReturnsOnlyRequestedAssets()
    {
        // Arrange
        WriteFile("agents/a.md", "---\nname: a\n---\n");
        WriteFile("instructions/general/code.instructions.md", "---\nname: code\n---\n");
        var sut = NewSut();

        // Act + Assert
        sut.All(AssetType.Agent).Should().HaveCount(1);
        sut.All(AssetType.Instruction).Should().HaveCount(1);
        sut.All().Should().HaveCount(2);
    }

    [Fact]
    public void Warnings_AfterBrokenFrontmatter_AreReported()
    {
        // Arrange
        WriteFile("agents/broken.md", "---\nname: : oops\n---\nBody");
        var sut = NewSut();

        // Act
        _ = sut.All();

        // Assert
        sut.Warnings.Should().NotBeEmpty();
    }

    [Fact]
    public void Find_WithUnknownAsset_ReturnsNull()
    {
        // Arrange
        WriteFile("agents/known.md", "---\nname: known\n---\n");
        var sut = NewSut();

        // Act
        var result = sut.Find(AssetType.Agent, "unknown");

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public void Find_WithForwardSlashIds_WorksRegardlessOfPathSeparator()
    {
        // Arrange
        WriteFile("agents/group/sub/leaf.md", "---\nname: leaf\n---\n");
        var sut = NewSut();

        // Act
        var result = sut.Find(AssetType.Agent, "group/sub/leaf");

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public void All_OnEmptyRepository_ReturnsEmpty()
    {
        // Arrange
        var sut = NewSut();

        // Act + Assert
        sut.All().Should().BeEmpty();
        sut.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void All_OnMcpConfigsFolder_IgnoresNonYamlFiles()
    {
        // Arrange
        WriteFile("mcp-configs/github.yaml", "name: github\ncommand: npx\n");
        WriteFile("mcp-configs/notes.txt", "ignored");
        var sut = NewSut();

        // Act
        var result = sut.All(AssetType.McpConfig);

        // Assert
        result.Should().ContainSingle();
    }

    [Fact]
    public void All_OnSkillsFolder_OnlyEntryFileTriggersDiscovery()
    {
        // Arrange
        WriteFile("skills/csharp/dotnet-tester/SKILL.md", "---\nname: dotnet-tester\n---\n");
        WriteFile("skills/csharp/loose-file.md", "not a skill");
        var sut = NewSut();

        // Act
        var result = sut.All(AssetType.Skill);

        // Assert
        result.Should().ContainSingle()
            .Which.Id.Should().Be("csharp/dotnet-tester");
    }

    private FileSystemAssetRepository NewSut() => new(new FileSystem(), _root);

    private void WriteFile(string relative, string content)
    {
        var full = Path.Combine(_root, relative);
        Directory.CreateDirectory(Path.GetDirectoryName(full)!);
        File.WriteAllText(full, content);
    }
}
