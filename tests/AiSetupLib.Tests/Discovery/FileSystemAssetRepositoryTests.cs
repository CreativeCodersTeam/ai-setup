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
    public void Find_DiscoversInstructionWithNestedPath()
    {
        WriteFile("instructions/csharp/csharp.instructions.md",
            "---\nname: csharp\ndescription: C#\ntargets: [copilot-cli]\n---\nBody");

        var sut = NewSut();

        var asset = sut.Find(AssetType.Instruction, "csharp/csharp.instructions");

        asset.Should().NotBeNull();
        asset!.Name.Should().Be("csharp");
        asset.Targets.Should().ContainSingle().Which.Should().Be(DeployTarget.CopilotCli);
        asset.Type.Should().Be(AssetType.Instruction);
    }

    [Fact]
    public void Find_DiscoversAgentAtRoot()
    {
        WriteFile("agents/dotnet-developer.md",
            "---\nname: dotnet-developer\ndescription: builds dotnet\n---\nBody");

        var sut = NewSut();

        sut.Find(AssetType.Agent, "dotnet-developer").Should().NotBeNull();
    }

    [Fact]
    public void Find_DiscoversSkillFolder()
    {
        WriteFile("skills/csharp/dotnet-tester/SKILL.md",
            "---\nname: dotnet-tester\ndescription: \"unit tests\"\n---\nBody");
        WriteFile("skills/csharp/dotnet-tester/references/xunit.md", "ref");

        var sut = NewSut();

        var asset = sut.Find(AssetType.Skill, "csharp/dotnet-tester");

        asset.Should().BeOfType<SkillAsset>();
        var skill = (SkillAsset)asset!;
        skill.Files.Should().Contain("SKILL.md");
        skill.Files.Should().Contain(s => s.EndsWith("xunit.md"));
    }

    [Fact]
    public void Find_DiscoversMcpConfig()
    {
        WriteFile("mcp-configs/github.yaml",
            "name: github\ntype: stdio\ncommand: npx\nargs:\n  - \"-y\"\n  - server\n");

        var sut = NewSut();

        sut.Find(AssetType.McpConfig, "github").Should().NotBeNull();
    }

    [Fact]
    public void All_FilteredByType_ReturnsOnlyRequested()
    {
        WriteFile("agents/a.md", "---\nname: a\n---\n");
        WriteFile("instructions/general/code.instructions.md", "---\nname: code\n---\n");

        var sut = NewSut();

        sut.All(AssetType.Agent).Should().HaveCount(1);
        sut.All(AssetType.Instruction).Should().HaveCount(1);
        sut.All().Should().HaveCount(2);
    }

    [Fact]
    public void Warnings_ReportedForBrokenFrontmatter()
    {
        WriteFile("agents/broken.md", "---\nname: : oops\n---\nBody");

        var sut = NewSut();

        _ = sut.All();

        sut.Warnings.Should().NotBeEmpty();
    }


    [Fact]
    public void Find_UnknownAsset_ReturnsNull()
    {
        WriteFile("agents/known.md", "---\nname: known\n---\n");

        var sut = NewSut();

        sut.Find(AssetType.Agent, "unknown").Should().BeNull();
    }

    [Fact]
    public void Find_UsesForwardSlashIds_RegardlessOfPathSeparator()
    {
        WriteFile("agents/group/sub/leaf.md", "---\nname: leaf\n---\n");

        var sut = NewSut();

        sut.Find(AssetType.Agent, "group/sub/leaf").Should().NotBeNull();
    }

    [Fact]
    public void All_RepositoryWithoutAnyFolders_ReturnsEmpty()
    {
        var sut = NewSut();

        sut.All().Should().BeEmpty();
        sut.Warnings.Should().BeEmpty();
    }

    [Fact]
    public void All_McpConfigsFolder_IgnoresNonYamlFiles()
    {
        WriteFile("mcp-configs/github.yaml", "name: github\ncommand: npx\n");
        WriteFile("mcp-configs/notes.txt", "ignored");

        var sut = NewSut();

        sut.All(AssetType.McpConfig).Should().ContainSingle();
    }

    [Fact]
    public void All_SkillsFolder_OnlyEntryFileTriggersDiscovery()
    {
        WriteFile("skills/csharp/dotnet-tester/SKILL.md", "---\nname: dotnet-tester\n---\n");
        WriteFile("skills/csharp/loose-file.md", "not a skill");

        var sut = NewSut();

        sut.All(AssetType.Skill).Should().ContainSingle()
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
