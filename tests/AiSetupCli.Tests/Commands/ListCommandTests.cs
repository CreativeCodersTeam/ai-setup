using AiSetup.Cli.Commands;
using AiSetup.Models;
using AiSetup.Platform;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace AiSetup.Cli.Tests.Commands;

public sealed class ListCommandTests
{
    [Fact]
    public void Execute_WithProfilesKind_RendersProfileTable()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var profilesDir = Path.Combine(sourceRepo, "profiles");
        A.CallTo(() => fs.DirectoryExists(profilesDir)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(profilesDir)).Returns(["dotnet-dev.yaml"]);
        A.CallTo(() => fs.ReadAllText(Path.Combine(profilesDir, "dotnet-dev.yaml")))
            .Returns("name: dotnet-dev\ndescription: dotnet stack\nagents:\n  - dotnet-developer\n");

        var console = new TestConsole();
        var sut = new ListCommand(fs, console);

        // Act
        var result = sut.Execute(NewContext(),
            new ListCommand.Settings { Kind = "profiles", SourceRepo = sourceRepo });

        // Assert
        result.Should().Be(0);
        console.Output.Should().Contain("dotnet-dev");
    }

    [Fact]
    public void Execute_WithAgentsKind_RendersFilteredAssetTable()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var agentsDir = Path.Combine(sourceRepo, "agents");
        A.CallTo(() => fs.DirectoryExists(agentsDir)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(agentsDir))
            .Returns(["dotnet-developer.md"]);
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentsDir, "dotnet-developer.md")))
            .Returns("---\nname: dotnet-developer\n---\nBody");

        var console = new TestConsole();
        var sut = new ListCommand(fs, console);

        // Act
        var result = sut.Execute(NewContext(),
            new ListCommand.Settings { Kind = "agents", SourceRepo = sourceRepo });

        // Assert
        result.Should().Be(0);
        console.Output.Should().Contain("dotnet-developer");
    }

    [Fact]
    public void Execute_WithoutKind_RendersAllAssets()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var agentsDir = Path.Combine(sourceRepo, "agents");
        A.CallTo(() => fs.DirectoryExists(agentsDir)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(agentsDir)).Returns(["a.md"]);
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentsDir, "a.md"))).Returns("---\nname: a\n---\n");

        var console = new TestConsole();
        var sut = new ListCommand(fs, console);

        // Act
        var result = sut.Execute(NewContext(), new ListCommand.Settings { SourceRepo = sourceRepo });

        // Assert
        result.Should().Be(0);
        console.Output.Should().Contain("a");
    }

    [Fact]
    public void Execute_WithTagFilter_ShowsOnlyMatchingAssets()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var agentsDir = Path.Combine(sourceRepo, "agents");
        A.CallTo(() => fs.DirectoryExists(agentsDir)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(agentsDir))
            .Returns(["tagged.md", "untagged.md"]);
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentsDir, "tagged.md")))
            .Returns("---\nname: tagged\ntags: [special]\n---\n");
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentsDir, "untagged.md")))
            .Returns("---\nname: untagged\n---\n");

        var console = new TestConsole();
        var sut = new ListCommand(fs, console);

        // Act
        sut.Execute(NewContext(),
            new ListCommand.Settings { Kind = "agents", Tag = "special", SourceRepo = sourceRepo });

        // Assert
        console.Output.Should().Contain("tagged");
        console.Output.Should().NotContain("untagged");
    }

    [Fact]
    public void Execute_WithTargetFilter_HidesAssetsWithMismatchedTargets()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var agentsDir = Path.Combine(sourceRepo, "agents");
        A.CallTo(() => fs.DirectoryExists(agentsDir)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(agentsDir))
            .Returns(["claude-only.md", "copilot-only.md"]);
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentsDir, "claude-only.md")))
            .Returns("---\nname: claude-only\ntargets: [claude-code]\n---\n");
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentsDir, "copilot-only.md")))
            .Returns("---\nname: copilot-only\ntargets: [copilot-cli]\n---\n");

        var console = new TestConsole();
        var sut = new ListCommand(fs, console);

        // Act
        sut.Execute(NewContext(),
            new ListCommand.Settings
            {
                Kind = "agents",
                Target = DeployTarget.ClaudeCode,
                SourceRepo = sourceRepo
            });

        // Assert
        console.Output.Should().Contain("claude-only");
        console.Output.Should().NotContain("copilot-only");
    }

    [Fact]
    public void Execute_WithUnknownKind_ThrowsAiSetupException()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var console = new TestConsole();
        var sut = new ListCommand(fs, console);

        // Act
        Action act = () => sut.Execute(NewContext(),
            new ListCommand.Settings { Kind = "wat", SourceRepo = sourceRepo });

        // Assert
        act.Should().Throw<AiSetup.Exceptions.AiSetupException>();
    }

    private static void ConfigureEmptyRepo(IFileSystem fs, string sourceRepo)
    {
        A.CallTo(() => fs.DirectoryExists(sourceRepo)).Returns(true);
        A.CallTo(() => fs.DirectoryExists(A<string>.That.StartsWith(sourceRepo + Path.DirectorySeparatorChar)))
            .Returns(false);
        A.CallTo(() => fs.EnumerateFilesRecursive(A<string>._)).Returns([]);
    }

    private static CommandContext NewContext() =>
        new(Array.Empty<string>(), A.Fake<IRemainingArguments>(), "list", null);
}
