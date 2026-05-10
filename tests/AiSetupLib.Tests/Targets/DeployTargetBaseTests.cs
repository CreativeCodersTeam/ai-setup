using AiSetup.Aggregation;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Profiles;
using AiSetup.Targets;

namespace AiSetup.Tests.Targets;

public sealed class DeployTargetBaseTests
{
    [Fact]
    public void Plan_RepoModeAssetWithoutDestination_ThrowsAiSetupException()
    {
        // Arrange
        var sut = NewSut(A.Fake<IFileSystem>(), A.Fake<IPathProvider>());

        // Act
        Action act = () => sut.Plan(
            NewOptions(),
            WithAgents((AssetType.Agent, "a", DeployMode.Repo)));

        // Assert
        act.Should().Throw<AiSetupException>().WithMessage("*DestinationRepoPath*");
    }

    [Fact]
    public void Plan_LocalOnlyProfile_DoesNotRequireDestination()
    {
        // Arrange
        var pathProvider = A.Fake<IPathProvider>();
        A.CallTo(() => pathProvider.GetLocalRoot(A<DeployTarget>._)).Returns("/local-root");
        var sut = NewSut(A.Fake<IFileSystem>(), pathProvider);

        // Act
        var plan = sut.Plan(
            NewOptions(),
            WithAgents((AssetType.Agent, "a", DeployMode.Local)));

        // Assert
        plan.Actions.Single().TargetPath.Should().StartWith("/local-root");
    }

    [Fact]
    public void Plan_RepoModeAsset_UsesDestinationAsRoot()
    {
        // Arrange
        var sut = NewSut(A.Fake<IFileSystem>(), A.Fake<IPathProvider>());

        // Act
        var plan = sut.Plan(
            NewOptions(destination: "/dest"),
            WithAgents((AssetType.Agent, "a", DeployMode.Repo)));

        // Assert
        plan.Actions.Should().ContainSingle()
            .Which.TargetPath.Should().StartWith("/dest");
    }

    [Fact]
    public void Plan_LocalModeAsset_DelegatesRootToPathProvider()
    {
        // Arrange
        var pathProvider = A.Fake<IPathProvider>();
        A.CallTo(() => pathProvider.GetLocalRoot(A<DeployTarget>._)).Returns("/local-root");
        var sut = NewSut(A.Fake<IFileSystem>(), pathProvider);

        // Act
        var plan = sut.Plan(
            NewOptions(destination: "/dest"),
            WithAgents((AssetType.Agent, "a", DeployMode.Local)));

        // Assert
        plan.Actions.Single().TargetPath.Should().StartWith("/local-root");
    }

    [Fact]
    public void Plan_MixedModes_ProducesActionsForBothRoots()
    {
        // Arrange
        var pathProvider = A.Fake<IPathProvider>();
        A.CallTo(() => pathProvider.GetLocalRoot(A<DeployTarget>._)).Returns("/local-root");
        var sut = NewSut(A.Fake<IFileSystem>(), pathProvider);

        // Act
        var plan = sut.Plan(
            NewOptions(destination: "/dest"),
            WithAgents(
                (AssetType.Agent, "repo-agent", DeployMode.Repo),
                (AssetType.Agent, "local-agent", DeployMode.Local)));

        // Assert
        plan.Actions.Should().HaveCount(2);
        plan.Actions.Should().Contain(a => a.TargetPath.StartsWith("/dest"));
        plan.Actions.Should().Contain(a => a.TargetPath.StartsWith("/local-root"));
    }

    [Fact]
    public void Plan_WhenFileExists_StatusIsOverwrite()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.FileExists(A<string>._)).Returns(true);
        var sut = NewSut(fs, A.Fake<IPathProvider>());

        // Act
        var plan = sut.Plan(
            NewOptions(destination: "/dest"),
            WithAgents((AssetType.Agent, "a", DeployMode.Repo)));

        // Assert
        plan.Actions.Single().Status.Should().Be(DeployActionStatus.Overwrite);
    }

    [Fact]
    public void Plan_WhenDirectoryExists_StatusIsOverwrite()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.FileExists(A<string>._)).Returns(false);
        A.CallTo(() => fs.DirectoryExists(A<string>._)).Returns(true);
        var sut = NewSut(fs, A.Fake<IPathProvider>());

        // Act
        var plan = sut.Plan(
            NewOptions(destination: "/dest"),
            WithAgents((AssetType.Agent, "a", DeployMode.Repo)));

        // Assert
        plan.Actions.Single().Status.Should().Be(DeployActionStatus.Overwrite);
    }

    [Fact]
    public void Plan_WhenNothingExists_StatusIsCreate()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.FileExists(A<string>._)).Returns(false);
        A.CallTo(() => fs.DirectoryExists(A<string>._)).Returns(false);
        var sut = NewSut(fs, A.Fake<IPathProvider>());

        // Act
        var plan = sut.Plan(
            NewOptions(destination: "/dest"),
            WithAgents((AssetType.Agent, "a", DeployMode.Repo)));

        // Assert
        plan.Actions.Single().Status.Should().Be(DeployActionStatus.Create);
    }

    [Fact]
    public void Plan_WithNullOptions_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = NewSut(A.Fake<IFileSystem>(), A.Fake<IPathProvider>());

        // Act
        Action act = () => sut.Plan(null!, ResolvedAssets.Empty);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Plan_WithNullAssets_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = NewSut(A.Fake<IFileSystem>(), A.Fake<IPathProvider>());

        // Act
        Action act = () => sut.Plan(NewOptions(destination: "/dest"), null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static TestTarget NewSut(IFileSystem fs, IPathProvider pp)
        => new(fs, pp, new MarkdownAggregator(), new McpConfigMerger(), new SettingsMerger());

    private static DeployOptions NewOptions(string? destination = null) => new()
    {
        Target = DeployTarget.ClaudeCode,
        SourceRepoPath = "/src",
        ProfileName = "test-profile",
        DestinationRepoPath = destination
    };

    private static ResolvedAssets WithAgents(params (AssetType Type, string Id, DeployMode Mode)[] agents)
        => new(
            Agents: agents.Select(a => new ResolvedAsset(NewAsset(a.Type, a.Id), a.Mode)).ToArray(),
            Instructions: [],
            Skills: [],
            McpConfigs: [],
            Settings: []);

    private static AssetDefinition NewAsset(AssetType type, string id) => new(
        id, type, id, string.Empty, [], [], "/" + id, null, new Dictionary<string, object?>(), $"# {id}");

    private sealed class TestTarget(
        IFileSystem fileSystem,
        IPathProvider pathProvider,
        IMarkdownAggregator markdownAggregator,
        IMcpConfigMerger mcpConfigMerger,
        ISettingsMerger settingsMerger)
        : DeployTargetBase(fileSystem, pathProvider, markdownAggregator, mcpConfigMerger, settingsMerger)
    {
        public override DeployTarget Target => DeployTarget.ClaudeCode;

        protected override void PlanInstructions(
            List<DeployAction> actions,
            DeployOptions options,
            DeployMode mode,
            IReadOnlyList<AssetDefinition> instructions,
            string root)
        {
        }

        protected override void PlanAgents(
            List<DeployAction> actions,
            DeployOptions options,
            DeployMode mode,
            IReadOnlyList<AssetDefinition> agents,
            string root)
        {
            foreach (var agent in agents)
            {
                var path = Path.Combine(root, agent.Id + ".md");
                actions.Add(new WriteFileAction(path, agent.Body, StatusFor(path), agent.Id));
            }
        }

        protected override void PlanSkills(
            List<DeployAction> actions,
            DeployOptions options,
            DeployMode mode,
            IReadOnlyList<AssetDefinition> skills,
            string root)
        {
        }

        protected override void PlanMcpConfigs(
            List<DeployAction> actions,
            DeployOptions options,
            DeployMode mode,
            IReadOnlyList<AssetDefinition> mcpConfigs,
            string root)
        {
        }

        protected override void PlanSettings(
            List<DeployAction> actions,
            DeployOptions options,
            DeployMode mode,
            IReadOnlyList<AssetDefinition> settings,
            string root)
        {
        }
    }
}
