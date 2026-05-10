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
        var plan = sut.Plan(RepoOptions(), new ResolvedAssets(
            Agents: [Repo(NewAsset(AssetType.Agent, "dotnet-developer"))],
            Instructions: [Repo(NewAsset(AssetType.Instruction, "csharp/csharp.instructions"))],
            Skills: [Repo(NewSkill("csharp/dotnet-tester", "/src/skills/csharp/dotnet-tester"))],
            McpConfigs: [],
            Settings: []));

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

        var sut = new ClaudeCodeTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

        // Act
        var plan = sut.Plan(LocalOptions(), new ResolvedAssets(
            [], [Local(NewAsset(AssetType.Instruction, "x"))], [], [], []));

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
        var plan = sut.Plan(RepoOptions(), new ResolvedAssets([], [], [],
            McpConfigs: [Repo(NewMcp("github", "name: github\ncommand: npx\n"))],
            Settings: []));

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
        var sut = new ClaudeCodeTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

        // Act
        var plan = sut.Plan(LocalOptions(), new ResolvedAssets(
            Agents: [Local(NewAsset(AssetType.Agent, "dotnet-developer"))],
            Instructions: [],
            Skills: [Local(NewSkill("csharp/dotnet-tester", "/src/skills/csharp/dotnet-tester"))],
            McpConfigs: [Local(NewMcp("github", "name: github\ncommand: npx\n"))],
            Settings: []));

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
    public void Plan_WithMixedModeInstructions_ProducesClaudeMdInBothRoots()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var pp = A.Fake<IPathProvider>();
        A.CallTo(() => pp.GetLocalRoot(DeployTarget.ClaudeCode)).Returns("/home/.claude");
        var sut = new ClaudeCodeTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

        // Act
        var plan = sut.Plan(RepoOptions(), new ResolvedAssets(
            Agents: [],
            Instructions: [
                Repo(NewAsset(AssetType.Instruction, "general/general.instructions")),
                Local(NewAsset(AssetType.Instruction, "csharp/csharp.instructions"))
            ],
            Skills: [],
            McpConfigs: [],
            Settings: []));

        // Assert
        var claudeMds = plan.Actions.OfType<WriteFileAction>()
            .Where(a => a.TargetPath.EndsWith("CLAUDE.md")).ToList();
        claudeMds.Should().HaveCount(2);
        claudeMds.Should().Contain(a => a.TargetPath.StartsWith("/dest"));
        claudeMds.Should().Contain(a => a.TargetPath.StartsWith("/home/.claude"));
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
                Repo(NewSkill("csharp/dotnet-tester", "/src/skills/csharp/dotnet-tester")),
                Repo(NewSkill("python/dotnet-tester", "/src/skills/python/dotnet-tester"))
            ],
            McpConfigs: [],
            Settings: []);

        // Act
        Action act = () => sut.Plan(RepoOptions(), assets);

        // Assert
        act.Should().Throw<Exceptions.AiSetupException>()
            .WithMessage("*Multiple skills resolve to*dotnet-tester*");
    }

    [Fact]
    public void Plan_WithSettingsFragment_MergesIntoClaudeSettingsJson()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        // Act
        var plan = sut.Plan(RepoOptions(), new ResolvedAssets([], [], [], [],
            Settings: [Repo(NewSettings("claude-code/base",
                """{ "model": "claude-opus-4-7", "permissions": { "allow": ["Bash(dotnet build:*)"] } }""",
                DeployTarget.ClaudeCode))]));

        // Assert
        var action = plan.Actions.OfType<WriteFileAction>().Single();
        action.TargetPath.Should().EndWith(Path.Combine(".claude", "settings.json"));
        action.Content.Should().Contain("\"model\": \"claude-opus-4-7\"");
        action.Content.Should().Contain("Bash(dotnet build:*)");
    }

    [Fact]
    public void Plan_WithMcpConfigAndSettingsFragment_ProducesSingleSettingsJsonWithBoth()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        // Act
        var plan = sut.Plan(RepoOptions(), new ResolvedAssets([], [], [],
            McpConfigs: [Repo(NewMcp("github", "name: github\ncommand: npx\n"))],
            Settings: [Repo(NewSettings("claude-code/base", """{ "model": "opus" }""", DeployTarget.ClaudeCode))]));

        // Assert
        var actions = plan.Actions.OfType<WriteFileAction>()
            .Where(a => a.TargetPath.EndsWith(Path.Combine(".claude", "settings.json"))).ToArray();
        actions.Should().ContainSingle();
        actions[0].Content.Should().Contain("\"mcpServers\"").And.Contain("\"model\": \"opus\"");
    }

    [Fact]
    public void Plan_WithSettingsFragmentTargetingCopilotOnly_IgnoresItForClaude()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        // Act
        var plan = sut.Plan(RepoOptions(), new ResolvedAssets([], [], [], [],
            Settings: [Repo(NewSettings("copilot-cli/x", """{ "model": "x" }""", DeployTarget.CopilotCli))]));

        // Assert
        plan.Actions.Should().BeEmpty();
    }

    private static ClaudeCodeTarget NewSut(IFileSystem fs)
        => new(fs, new PathProvider(), new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

    private static DeployOptions RepoOptions() => new()
    {
        Target = DeployTarget.ClaudeCode,
        SourceRepoPath = "/src",
        ProfileName = "test-profile",
        DestinationRepoPath = "/dest"
    };

    private static DeployOptions LocalOptions() => new()
    {
        Target = DeployTarget.ClaudeCode,
        SourceRepoPath = "/src",
        ProfileName = "test-profile"
    };

    private static ResolvedAsset Repo(AssetDefinition definition) => new(definition, DeployMode.Repo);

    private static ResolvedAsset Local(AssetDefinition definition) => new(definition, DeployMode.Local);

    private static AssetDefinition NewAsset(AssetType type, string id) => new(
        id, type, id, string.Empty, [], [], "/" + id, null, new Dictionary<string, object?>(), $"# {id}");

    private static SkillAsset NewSkill(string id, string folder) => new(
        id, id, string.Empty, [], [], folder + "/SKILL.md", null,
        new Dictionary<string, object?>(), "body", folder, ["SKILL.md"]);

    private static AssetDefinition NewMcp(string id, string body) => new(
        id, AssetType.McpConfig, id, string.Empty, [], [], "/" + id, null,
        new Dictionary<string, object?>(), body);

    private static AssetDefinition NewSettings(string id, string body, DeployTarget target) => new(
        id, AssetType.Settings, Path.GetFileName(id), string.Empty, [], [target],
        "/src/settings/" + id + ".json", null, new Dictionary<string, object?>(), body);
}
