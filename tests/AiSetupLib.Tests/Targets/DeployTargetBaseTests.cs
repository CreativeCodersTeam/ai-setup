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
    public void Plan_RepoModeWithoutDestination_ThrowsAiSetupException()
    {
        // Arrange
        var sut = NewSut(A.Fake<IFileSystem>(), A.Fake<IPathProvider>());

        // Act
        Action act = () => sut.Plan(
            new DeployOptions
            {
                Target = DeployTarget.ClaudeCode,
                Mode = DeployMode.Repo,
                SourceRepoPath = "/src"
            },
            ResolvedAssets.Empty);

        // Assert
        act.Should().Throw<AiSetupException>().WithMessage("*DestinationRepoPath*");
    }

    [Fact]
    public void Plan_RepoMode_UsesDestinationAsRoot()
    {
        // Arrange
        var sut = NewSut(A.Fake<IFileSystem>(), A.Fake<IPathProvider>());

        // Act
        var plan = sut.Plan(
            new DeployOptions
            {
                Target = DeployTarget.ClaudeCode,
                Mode = DeployMode.Repo,
                SourceRepoPath = "/src",
                DestinationRepoPath = "/dest"
            },
            new ResolvedAssets(
                Agents: [NewAsset(AssetType.Agent, "a")], Instructions: [], Skills: [], McpConfigs: []));

        // Assert
        plan.Actions.Should().ContainSingle()
            .Which.TargetPath.Should().StartWith("/dest");
    }

    [Fact]
    public void Plan_LocalMode_DelegatesRootToPathProvider()
    {
        // Arrange
        var pathProvider = A.Fake<IPathProvider>();
        A.CallTo(() => pathProvider.GetLocalRoot(A<DeployTarget>._)).Returns("/local-root");
        var sut = NewSut(A.Fake<IFileSystem>(), pathProvider);

        // Act
        var plan = sut.Plan(
            new DeployOptions
            {
                Target = DeployTarget.ClaudeCode,
                Mode = DeployMode.Local,
                SourceRepoPath = "/src"
            },
            new ResolvedAssets(
                Agents: [NewAsset(AssetType.Agent, "a")], Instructions: [], Skills: [], McpConfigs: []));

        // Assert
        plan.Actions.Single().TargetPath.Should().StartWith("/local-root");
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
            new DeployOptions
            {
                Target = DeployTarget.ClaudeCode,
                Mode = DeployMode.Repo,
                SourceRepoPath = "/src",
                DestinationRepoPath = "/dest"
            },
            new ResolvedAssets(
                Agents: [NewAsset(AssetType.Agent, "a")], Instructions: [], Skills: [], McpConfigs: []));

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
            new DeployOptions
            {
                Target = DeployTarget.ClaudeCode,
                Mode = DeployMode.Repo,
                SourceRepoPath = "/src",
                DestinationRepoPath = "/dest"
            },
            new ResolvedAssets(
                Agents: [NewAsset(AssetType.Agent, "a")], Instructions: [], Skills: [], McpConfigs: []));

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
            new DeployOptions
            {
                Target = DeployTarget.ClaudeCode,
                Mode = DeployMode.Repo,
                SourceRepoPath = "/src",
                DestinationRepoPath = "/dest"
            },
            new ResolvedAssets(
                Agents: [NewAsset(AssetType.Agent, "a")], Instructions: [], Skills: [], McpConfigs: []));

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
        Action act = () => sut.Plan(
            new DeployOptions
            {
                Target = DeployTarget.ClaudeCode,
                Mode = DeployMode.Repo,
                SourceRepoPath = "/src",
                DestinationRepoPath = "/dest"
            },
            null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    private static TestTarget NewSut(IFileSystem fs, IPathProvider pp)
        => new(fs, pp, new MarkdownAggregator(), new McpConfigMerger());

    private static AssetDefinition NewAsset(AssetType type, string id) => new(
        id, type, id, string.Empty, [], [], "/" + id, null, new Dictionary<string, object?>(), $"# {id}");

    private sealed class TestTarget(
        IFileSystem fileSystem,
        IPathProvider pathProvider,
        IMarkdownAggregator markdownAggregator,
        IMcpConfigMerger mcpConfigMerger)
        : DeployTargetBase(fileSystem, pathProvider, markdownAggregator, mcpConfigMerger)
    {
        public override DeployTarget Target => DeployTarget.ClaudeCode;

        protected override void PlanInstructions(
            List<DeployAction> actions,
            DeployOptions options,
            IReadOnlyList<AssetDefinition> instructions,
            string root)
        {
        }

        protected override void PlanAgents(
            List<DeployAction> actions,
            DeployOptions options,
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
            IReadOnlyList<AssetDefinition> skills,
            string root)
        {
        }

        protected override void PlanMcpConfigs(
            List<DeployAction> actions,
            DeployOptions options,
            IReadOnlyList<AssetDefinition> mcpConfigs,
            string root)
        {
        }
    }
}
