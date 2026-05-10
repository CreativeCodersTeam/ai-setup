using AiSetup.Aggregation;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;

namespace AiSetup.Tests.Targets;

public sealed class ClaudeCodeTargetTests
{
    [Fact]
    public void Plan_InRepoMode_AggregatesInstructionsToClaudeMd()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        // Act
        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "test-profile",
            DestinationRepoPath = "/dest"
        }, new ResolvedAssets(
            Agents: [NewAsset(AssetType.Agent, "dotnet-developer")],
            Instructions: [NewAsset(AssetType.Instruction, "csharp/csharp.instructions")],
            Skills: [NewSkill("csharp/dotnet-tester", "/src/skills/csharp/dotnet-tester")],
            McpConfigs: []));

        // Assert
        plan.Actions.Should().Contain(a =>
            a is WriteFileAction && a.TargetPath.EndsWith("CLAUDE.md"));

        plan.Actions.Should().Contain(a =>
            a.TargetPath.Contains(Path.Combine(".claude", "agents", "dotnet-developer.md")));

        plan.Actions.Should().Contain(a =>
            a is CopyDirectoryAction && a.TargetPath.EndsWith(Path.Combine(".claude", "skills", "dotnet-tester")));
    }

    [Fact]
    public void Plan_InLocalModeWithExistingClaudeMd_BackupsExistingFile()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var pp = A.Fake<IPathProvider>();
        A.CallTo(() => pp.GetLocalRoot(DeployTarget.ClaudeCode)).Returns("/home/.claude");
        A.CallTo(() => fs.FileExists(Path.Combine("/home/.claude", "CLAUDE.md"))).Returns(true);

        var sut = new ClaudeCodeTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger());

        // Act
        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Local,
            SourceRepoPath = "/src",
            ProfileName = "test-profile"
        }, new ResolvedAssets([], [NewAsset(AssetType.Instruction, "x")], [], []));

        // Assert
        plan.Actions.OfType<BackupFileAction>().Should().ContainSingle()
            .Which.TargetPath.Should().EndWith("CLAUDE.md.bak");
    }

    [Fact]
    public void Plan_WithMcpConfig_WritesToClaudeSettingsJsonWithMcpServersKey()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        // Act
        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "test-profile",
            DestinationRepoPath = "/dest"
        }, new ResolvedAssets([], [], [],
            McpConfigs: [NewMcp("github", "name: github\ncommand: npx\n")]));

        // Assert
        var action = plan.Actions.OfType<WriteFileAction>().Single();
        action.TargetPath.Should().EndWith(Path.Combine(".claude", "settings.json"));
        action.Content.Should().Contain("\"mcpServers\"");
    }

    [Fact]
    public void Plan_InLocalMode_DoesNotDoubleClaudeFolderAndStripsOrgFolder()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var pp = A.Fake<IPathProvider>();
        A.CallTo(() => pp.GetLocalRoot(DeployTarget.ClaudeCode)).Returns("/home/.claude");
        var sut = new ClaudeCodeTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger());

        // Act
        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Local,
            SourceRepoPath = "/src",
            ProfileName = "test-profile"
        }, new ResolvedAssets(
            Agents: [NewAsset(AssetType.Agent, "dotnet-developer")],
            Instructions: [],
            Skills: [NewSkill("csharp/dotnet-tester", "/src/skills/csharp/dotnet-tester")],
            McpConfigs: [NewMcp("github", "name: github\ncommand: npx\n")]));

        // Assert
        plan.Actions.OfType<WriteFileAction>().Should().Contain(a =>
            a.TargetPath == Path.Combine("/home/.claude", "agents", "dotnet-developer.md"));

        plan.Actions.OfType<CopyDirectoryAction>().Should().ContainSingle()
            .Which.TargetPath.Should().Be(Path.Combine("/home/.claude", "skills", "dotnet-tester"));

        plan.Actions.OfType<WriteFileAction>().Should().Contain(a =>
            a.TargetPath == Path.Combine("/home/.claude", "settings.json"));

        plan.Actions.Should().NotContain(a => a.TargetPath.Contains(Path.Combine(".claude", ".claude")));
    }

    [Fact]
    public void Plan_WithCollidingSkillLeafIds_ThrowsAiSetupException()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        var assets = new ResolvedAssets(
            Agents: [], Instructions: [],
            Skills: [
                NewSkill("csharp/dotnet-tester", "/src/skills/csharp/dotnet-tester"),
                NewSkill("python/dotnet-tester", "/src/skills/python/dotnet-tester")
            ],
            McpConfigs: []);

        // Act
        Action act = () => sut.Plan(new DeployOptions
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "test-profile",
            DestinationRepoPath = "/dest"
        }, assets);

        // Assert
        act.Should().Throw<Exceptions.AiSetupException>()
            .WithMessage("*Multiple skills resolve to*dotnet-tester*");
    }

    private static ClaudeCodeTarget NewSut(IFileSystem fs)
        => new(fs, new PathProvider(), new MarkdownAggregator(), new McpConfigMerger());

    private static AssetDefinition NewAsset(AssetType type, string id) => new(
        id, type, id, string.Empty, [], [], "/" + id, null, new Dictionary<string, object?>(), $"# {id}");

    private static SkillAsset NewSkill(string id, string folder) => new(
        id, id, string.Empty, [], [], folder + "/SKILL.md", null,
        new Dictionary<string, object?>(), "body", folder, ["SKILL.md"]);

    private static AssetDefinition NewMcp(string id, string body) => new(
        id, AssetType.McpConfig, id, string.Empty, [], [], "/" + id, null,
        new Dictionary<string, object?>(), body);
}
