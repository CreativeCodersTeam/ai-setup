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
            Agents: [NewAsset(AssetType.Agent, "dotnet-developer")],
            Instructions: [NewAsset(AssetType.Instruction, "csharp/csharp.instructions")],
            Skills: [NewSkill("csharp/dotnet-tester", "/repo/skills/csharp/dotnet-tester")],
            McpConfigs: []);

        // Act
        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.CopilotCli,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "test-profile",
            DestinationRepoPath = "/dest"
        }, assets);

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
        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.CopilotCli,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "test-profile",
            DestinationRepoPath = "/dest"
        }, new ResolvedAssets([], [], [],
            McpConfigs: [NewMcp("github", "name: github\ncommand: npx\n")]));

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

        var sut = new CopilotCliTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger());

        // Act
        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.CopilotCli,
            Mode = DeployMode.Local,
            SourceRepoPath = "/src",
            ProfileName = "test-profile"
        }, new ResolvedAssets(
            Agents: [NewAsset(AssetType.Agent, "x")], Instructions: [], Skills: [], McpConfigs: []));

        // Assert
        plan.Actions.Single().TargetPath.Should().Be(Path.Combine("/local/copilot", "agents", "x.md"));
    }

    [Fact]
    public void Plan_WithCollidingAgentLeafIds_ThrowsAiSetupException()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        var assets = new ResolvedAssets(
            Agents: [
                NewAsset(AssetType.Agent, "csharp/dotnet-tester"),
                NewAsset(AssetType.Agent, "python/dotnet-tester")
            ],
            Instructions: [], Skills: [], McpConfigs: []);

        // Act
        Action act = () => sut.Plan(new DeployOptions
        {
            Target = DeployTarget.CopilotCli,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            ProfileName = "test-profile",
            DestinationRepoPath = "/dest"
        }, assets);

        // Assert
        act.Should().Throw<Exceptions.AiSetupException>()
            .WithMessage("*Multiple agents resolve to*dotnet-tester.md*");
    }

    private static CopilotCliTarget NewSut(IFileSystem fs)
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
