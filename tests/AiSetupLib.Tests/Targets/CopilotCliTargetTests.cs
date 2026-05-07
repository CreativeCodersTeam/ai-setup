using AiSetup.Aggregation;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;

namespace AiSetup.Tests.Targets;

public sealed class CopilotCliTargetTests
{
    [Fact]
    public void Plan_RepoMode_PlacesAssetsUnderDotGithub()
    {
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        var assets = new ResolvedAssets(
            Agents: [NewAsset(AssetType.Agent, "dotnet-developer")],
            Instructions: [NewAsset(AssetType.Instruction, "csharp/csharp.instructions")],
            Skills: [NewSkill("csharp/dotnet-tester", "/repo/skills/csharp/dotnet-tester")],
            McpConfigs: []);

        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.CopilotCli,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            DestinationRepoPath = "/dest"
        }, assets);

        plan.Actions.Should().Contain(a => a.TargetPath.Contains(Path.Combine(".github", "instructions", "csharp.instructions.md")));
        plan.Actions.Should().Contain(a => a.TargetPath.Contains(Path.Combine(".github", "agents", "dotnet-developer.md")));
        plan.Actions.Should().Contain(a => a is CopyDirectoryAction && a.TargetPath.EndsWith(Path.Combine(".github", "skills", "dotnet-tester")));
    }

    [Fact]
    public void Plan_RepoMode_McpGoesToVsCodeMcpJson()
    {
        var fs = A.Fake<IFileSystem>();
        var sut = NewSut(fs);

        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.CopilotCli,
            Mode = DeployMode.Repo,
            SourceRepoPath = "/src",
            DestinationRepoPath = "/dest"
        }, new ResolvedAssets([], [], [],
            McpConfigs: [NewMcp("github", "name: github\ncommand: npx\n")]));

        var action = plan.Actions.OfType<WriteFileAction>().Single();
        action.TargetPath.Should().EndWith(Path.Combine(".vscode", "mcp.json"));
        action.Content.Should().Contain("\"servers\"");
        action.Content.Should().Contain("\"github\"");
    }

    [Fact]
    public void Plan_LocalMode_UsesPathProviderRoot()
    {
        var fs = A.Fake<IFileSystem>();
        var pp = A.Fake<IPathProvider>();
        A.CallTo(() => pp.GetLocalRoot(DeployTarget.CopilotCli)).Returns("/local/copilot");

        var sut = new CopilotCliTarget(fs, pp, new MarkdownAggregator(), new McpConfigMerger());

        var plan = sut.Plan(new DeployOptions
        {
            Target = DeployTarget.CopilotCli,
            Mode = DeployMode.Local,
            SourceRepoPath = "/src"
        }, new ResolvedAssets(
            Agents: [NewAsset(AssetType.Agent, "x")], Instructions: [], Skills: [], McpConfigs: []));

        plan.Actions.Single().TargetPath.Should().Be(Path.Combine("/local/copilot", "agents", "x.md"));
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
