using AiSetup;
using AiSetup.Aggregation;
using AiSetup.Cli.Commands;
using AiSetup.Cli.Rendering;
using AiSetup.Models;
using AiSetup.Platform;
using AiSetup.Targets;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace AiSetup.Cli.Tests.Commands;

public sealed class DeployCommandTests
{
    [Fact]
    public void Execute_WithDryRun_ReturnsZeroAndPrintsDryRun()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepo = sourceRepo,
            DestinationRepo = "/dest",
            DryRun = true
        });

        // Assert
        result.Should().Be(0);
        console.Output.Should().Contain("Dry-run");
    }

    [Fact]
    public void Execute_WithNonExistentSourceRepo_ThrowsAiSetupException()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.DirectoryExists("/missing")).Returns(false);

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        Action act = () => sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepo = "/missing",
            DestinationRepo = "/dest",
            DryRun = true
        });

        // Assert
        act.Should().Throw<AiSetup.Exceptions.AiSetupException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void Execute_RepoModeWithoutDestination_PrintsErrorAndReturnsTwo()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepo = sourceRepo,
            DestinationRepo = null,
            DryRun = true
        });

        // Assert
        result.Should().Be(2);
        console.Output.Should().Contain("DestinationRepoPath");
    }

    [Fact]
    public void Execute_LocalModeWithSelectedAgent_ProducesPlanAndReturnsZero()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var agentsDir = Path.Combine(sourceRepo, "agents");
        A.CallTo(() => fs.DirectoryExists(agentsDir)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(agentsDir)).Returns(["a.md"]);
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentsDir, "a.md")))
            .Returns("---\nname: a\n---\nbody");

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Local,
            SourceRepo = sourceRepo,
            Agents = ["a"],
            DryRun = true
        });

        // Assert
        result.Should().Be(0);
        console.Output.Should().Contain("a.md");
    }

    [Fact]
    public void Execute_WithUnknownAsset_PrintsErrorAndReturnsTwo()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            SourceRepo = sourceRepo,
            DestinationRepo = "/dest",
            Skills = ["does/not-exist"],
            DryRun = true
        });

        // Assert
        result.Should().Be(2);
        console.Output.Should().Contain("not found");
    }

    [Fact]
    public void Validate_WithoutTarget_ReturnsError()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Validate(NewContext(), new DeployCommand.Settings
        {
            Mode = DeployMode.Repo,
            DestinationRepo = "/dest"
        });

        // Assert
        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("--target is required");
    }

    [Fact]
    public void Validate_WithTargetAndRepoMode_ReturnsSuccessWhenDestinationProvided()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Validate(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            Mode = DeployMode.Repo,
            DestinationRepo = "/dest"
        });

        // Assert
        result.Successful.Should().BeTrue();
    }

    private static DeployCommand NewSut(IFileSystem fs, TestConsole console)
    {
        var pathProvider = new PathProvider();
        var aggregator = new MarkdownAggregator();
        var merger = new McpConfigMerger();
        var registry = new TargetRegistry(new IDeployTarget[]
        {
            new CopilotCliTarget(fs, pathProvider, aggregator, merger),
            new ClaudeCodeTarget(fs, pathProvider, aggregator, merger),
        });
        return new DeployCommand(fs, new RepositoryFactory(fs), registry, console, new PlanRenderer(console));
    }

    private static void ConfigureEmptyRepo(IFileSystem fs, string sourceRepo)
    {
        A.CallTo(() => fs.DirectoryExists(sourceRepo)).Returns(true);
        A.CallTo(() => fs.DirectoryExists(A<string>.That.StartsWith(sourceRepo + Path.DirectorySeparatorChar)))
            .Returns(false);
        A.CallTo(() => fs.EnumerateFilesRecursive(A<string>._)).Returns([]);
    }

    private static CommandContext NewContext() =>
        new(Array.Empty<string>(), A.Fake<IRemainingArguments>(), "deploy", null);
}
