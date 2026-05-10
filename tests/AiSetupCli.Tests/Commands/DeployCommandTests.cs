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
        ConfigureProfiles(fs, sourceRepo, ("dev.yaml", "name: dev"));

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            SourceRepo = sourceRepo,
            DestinationRepo = "/dest",
            Profile = "dev",
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
            SourceRepo = "/missing",
            DestinationRepo = "/dest",
            Profile = "dev",
            DryRun = true
        });

        // Assert
        act.Should().Throw<AiSetup.Exceptions.AiSetupException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void Execute_RepoModeAssetWithoutDestination_PrintsErrorAndReturnsTwo()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);
        ConfigureAgent(fs, sourceRepo, "a");
        ConfigureProfiles(fs, sourceRepo, ("dev.yaml", "name: dev\nagents:\n  - a\n"));

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            SourceRepo = sourceRepo,
            DestinationRepo = null,
            Profile = "dev",
            DryRun = true
        });

        // Assert
        result.Should().Be(2);
        console.Output.Should().Contain("DestinationRepoPath");
    }

    [Fact]
    public void Execute_LocalModeAssetWithProfile_ProducesPlanAndReturnsZero()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);
        ConfigureAgent(fs, sourceRepo, "a");
        ConfigureProfiles(fs, sourceRepo, ("dev.yaml", "name: dev\nagents:\n  - a@local\n"));

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            SourceRepo = sourceRepo,
            Profile = "dev",
            DryRun = true
        });

        // Assert
        result.Should().Be(0);
        console.Output.Should().Contain("a.md");
    }

    [Fact]
    public void Execute_WithMixedModeProfile_PlansBothAssetsAndReturnsZero()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);
        ConfigureAgent(fs, sourceRepo, "local-agent");
        ConfigureAgent(fs, sourceRepo, "repo-agent");
        A.CallTo(() => fs.EnumerateFilesRecursive(Path.Combine(sourceRepo, "agents")))
            .Returns(["local-agent.md", "repo-agent.md"]);
        ConfigureProfiles(fs, sourceRepo, ("dev.yaml", "name: dev\nagents:\n  - local-agent@local\n  - repo-agent@repo\n"));

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            SourceRepo = sourceRepo,
            DestinationRepo = "/dest",
            Profile = "dev",
            DryRun = true
        });

        // Assert
        result.Should().Be(0);
        console.Output.Should().Contain("local-agent.md");
        console.Output.Should().Contain("repo-agent.md");
    }

    [Fact]
    public void Execute_WithUnknownProfile_PrintsErrorAndReturnsTwo()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);
        ConfigureProfiles(fs, sourceRepo, ("dotnet-dev.yaml", "name: dotnet-dev"));

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            SourceRepo = sourceRepo,
            DestinationRepo = "/dest",
            Profile = "dotnet-de",
            DryRun = true
        });

        // Assert
        result.Should().Be(2);
        console.Output.Should().Contain("was not found");
    }

    [Fact]
    public void Execute_WithProfileReferencingUnknownAsset_PrintsErrorAndReturnsTwo()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);
        ConfigureProfiles(fs, sourceRepo, ("dev.yaml", "name: dev\nskills:\n  - does/not-exist\n"));

        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            SourceRepo = sourceRepo,
            DestinationRepo = "/dest",
            Profile = "dev",
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
            DestinationRepo = "/dest",
            Profile = "dev"
        });

        // Assert
        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("--target is required");
    }

    [Fact]
    public void Validate_WithoutProfile_ReturnsError()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Validate(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            DestinationRepo = "/dest"
        });

        // Assert
        result.Successful.Should().BeFalse();
        result.Message.Should().Contain("--profile is required");
    }

    [Fact]
    public void Validate_WithTargetAndProfile_ReturnsSuccess()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var console = new TestConsole();
        var sut = NewSut(fs, console);

        // Act
        var result = sut.Validate(NewContext(), new DeployCommand.Settings
        {
            Target = DeployTarget.ClaudeCode,
            DestinationRepo = "/dest",
            Profile = "dev"
        });

        // Assert
        result.Successful.Should().BeTrue();
    }

    private static DeployCommand NewSut(IFileSystem fs, TestConsole console)
    {
        var pathProvider = new PathProvider();
        var aggregator = new MarkdownAggregator();
        var merger = new McpConfigMerger();
        var settingsMerger = new SettingsMerger();
        var registry = new TargetRegistry(new IDeployTarget[]
        {
            new CopilotCliTarget(fs, pathProvider, aggregator, merger, settingsMerger),
            new ClaudeCodeTarget(fs, pathProvider, aggregator, merger, settingsMerger),
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

    private static void ConfigureProfiles(
        IFileSystem fs, string sourceRepo, params (string FileName, string Yaml)[] profiles)
    {
        var profilesDir = Path.Combine(sourceRepo, "profiles");
        A.CallTo(() => fs.DirectoryExists(profilesDir)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(profilesDir)).Returns(profiles.Select(p => p.FileName).ToArray());

        foreach (var (fileName, yaml) in profiles)
        {
            A.CallTo(() => fs.ReadAllText(Path.Combine(profilesDir, fileName))).Returns(yaml);
        }
    }

    private static void ConfigureAgent(IFileSystem fs, string sourceRepo, string name)
    {
        var agentsDir = Path.Combine(sourceRepo, "agents");
        A.CallTo(() => fs.DirectoryExists(agentsDir)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(agentsDir)).Returns([$"{name}.md"]);
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentsDir, $"{name}.md")))
            .Returns($"---\nname: {name}\n---\nbody");
    }

    private static CommandContext NewContext() =>
        new(Array.Empty<string>(), A.Fake<IRemainingArguments>(), "deploy", null);
}
