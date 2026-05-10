using AiSetup.Aggregation;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;

namespace AiSetup.Tests.Targets;

public sealed class CopilotCliTargetTests
{
    [Fact]
    public void Plan_InRepoMode_PlacesAssetsUnderDotGithub()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        var assets = new ResolvedAssets(
            Agents: [Repo(NewAsset(AssetType.Agent, "dotnet-developer"))],
            Instructions: [Repo(NewAsset(AssetType.Instruction, "csharp/csharp.instructions"))],
            Skills: [Repo(NewSkill("csharp/dotnet-tester", "/repo/skills/csharp/dotnet-tester"))],
            McpConfigs: [],
            Settings: []);

        // Act
        var plan = sut.Plan(RepoOptions(), assets);

        // Assert
        plan.Actions.Should().Contain(a => a.TargetPath.Contains(Path.Combine(".github", "instructions", "csharp.instructions.md")));
        plan.Actions.Should().Contain(a => a.TargetPath.Contains(Path.Combine(".github", "agents", "dotnet-developer.md")));
        plan.Actions.Should().Contain(a => a is CopyDirectoryAction && a.TargetPath.EndsWith(Path.Combine(".github", "skills", "dotnet-tester")));
    }

    [Fact]
    public void Plan_InRepoModeWithMcp_WritesToDotGithubCopilotMcpJson()
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
        action.TargetPath.Should().EndWith(Path.Combine(".github", "copilot", "mcp.json"));
        action.Content.Should().Contain("\"servers\"");
        action.Content.Should().Contain("\"github\"");
    }

    [Fact]
    public void Plan_InLocalMode_UsesPathProviderRoot()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var pp = A.Fake<IPathProvider>();
        A.CallTo(() => pp.GetLocalRoot(DeployTarget.CopilotCli)).Returns("/local/copilot");

        var sut = new CopilotCliTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

        // Act
        var plan = sut.Plan(LocalOptions(), new ResolvedAssets(
            Agents: [Local(NewAsset(AssetType.Agent, "x"))], Instructions: [], Skills: [], McpConfigs: [], Settings: []));

        // Assert
        plan.Actions.Single().TargetPath.Should().Be(Path.Combine("/local/copilot", "agents", "x.md"));
    }

    [Fact]
    public void Plan_WithMixedModeAgents_PlacesEachUnderItsOwnRoot()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var pp = A.Fake<IPathProvider>();
        A.CallTo(() => pp.GetLocalRoot(DeployTarget.CopilotCli)).Returns("/local/copilot");
        var sut = new CopilotCliTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

        // Act
        var plan = sut.Plan(RepoOptions(), new ResolvedAssets(
            Agents: [Repo(NewAsset(AssetType.Agent, "repo-agent")), Local(NewAsset(AssetType.Agent, "local-agent"))],
            Instructions: [], Skills: [], McpConfigs: [], Settings: []));

        // Assert
        plan.Actions.Should().Contain(a => a.TargetPath == Path.Combine("/dest", ".github", "agents", "repo-agent.md"));
        plan.Actions.Should().Contain(a => a.TargetPath == Path.Combine("/local/copilot", "agents", "local-agent.md"));
    }

    [Fact]
    public void Plan_WithCollidingAgentLeafIds_ThrowsAiSetupException()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        var assets = new ResolvedAssets(
            Agents: [
                Repo(NewAsset(AssetType.Agent, "csharp/dotnet-tester")),
                Repo(NewAsset(AssetType.Agent, "python/dotnet-tester"))
            ],
            Instructions: [], Skills: [], McpConfigs: [], Settings: []);

        // Act
        Action act = () => sut.Plan(RepoOptions(), assets);

        // Assert
        act.Should().Throw<Exceptions.AiSetupException>()
            .WithMessage("*Multiple agents resolve to*dotnet-tester.md*");
    }

    [Fact]
    public void Plan_WithSettingsFragmentInLocalMode_MergesIntoCopilotSettingsJson()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var pp = A.Fake<IPathProvider>();
        A.CallTo(() => pp.GetLocalRoot(DeployTarget.CopilotCli)).Returns("/home/.copilot");
        var sut = new CopilotCliTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

        // Act
        var plan = sut.Plan(LocalOptions(), new ResolvedAssets([], [], [], [],
            Settings: [Local(NewSettings("copilot-cli/defaults", """{ "model": "claude-opus-4.7" }""", DeployTarget.CopilotCli))]));

        // Assert
        var action = plan.Actions.OfType<WriteFileAction>()
            .Single(a => a.TargetPath.EndsWith("settings.json"));
        action.TargetPath.Should().Be(Path.Combine("/home/.copilot", "settings.json"));
        action.Content.Should().Contain("\"model\": \"claude-opus-4.7\"");
    }

    [Fact]
    public void Plan_WithSettingsFragmentInRepoMode_DoesNotWriteSettings()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        // Act
        var plan = sut.Plan(RepoOptions(), new ResolvedAssets([], [], [], [],
            Settings: [Repo(NewSettings("copilot-cli/defaults", """{ "model": "x" }""", DeployTarget.CopilotCli))]));

        // Assert
        plan.Actions.Should().NotContain(a => a.TargetPath.EndsWith("settings.json"));
    }

    private static CopilotCliTarget NewSut(IFileSystem fs)
        => new(fs, new PathProvider(), new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

    private static DeployOptions RepoOptions() => new()
    {
        Target = DeployTarget.CopilotCli,
        SourceRepoPath = "/src",
        ProfileName = "test-profile",
        DestinationRepoPath = "/dest"
    };

    private static DeployOptions LocalOptions() => new()
    {
        Target = DeployTarget.CopilotCli,
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
