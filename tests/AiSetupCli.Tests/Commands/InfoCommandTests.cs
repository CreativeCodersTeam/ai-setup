using AiSetup.Cli.Commands;
using AiSetup.Platform;
using Spectre.Console.Cli;
using Spectre.Console.Testing;

namespace AiSetup.Cli.Tests.Commands;

public sealed class InfoCommandTests
{
    [Fact]
    public void Execute_BlankAssetId_ReturnsValidationError()
    {
        var fs = A.Fake<IFileSystem>();
        var console = new TestConsole();
        var sut = new InfoCommand(fs, console);

        var result = sut.Execute(NewContext(), new InfoCommand.Settings { AssetId = "  " });

        result.Should().Be(2);
        console.Output.Should().Contain("asset ID is required");
    }

    [Fact]
    public void Execute_SourceRepoMissing_Throws()
    {
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.DirectoryExists(A<string>._)).Returns(false);
        var console = new TestConsole();
        var sut = new InfoCommand(fs, console);

        Action act = () => sut.Execute(NewContext(),
            new InfoCommand.Settings { AssetId = "x", SourceRepo = "/missing" });

        act.Should().Throw<AiSetup.Exceptions.AiSetupException>()
            .WithMessage("*does not exist*");
    }

    [Fact]
    public void Execute_NotFound_ReturnsOne()
    {
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var console = new TestConsole();
        var sut = new InfoCommand(fs, console);

        var result = sut.Execute(NewContext(),
            new InfoCommand.Settings { AssetId = "missing/asset", SourceRepo = sourceRepo });

        result.Should().Be(1);
        console.Output.Should().Contain("missing/asset");
        console.Output.Should().Contain("not found");
    }

    [Fact]
    public void Execute_AssetFound_RendersDetailsAndReturnsZero()
    {
        var fs = A.Fake<IFileSystem>();
        var sourceRepo = "/repo";
        ConfigureEmptyRepo(fs, sourceRepo);

        var agentRoot = Path.Combine(sourceRepo, "agents");
        A.CallTo(() => fs.DirectoryExists(agentRoot)).Returns(true);
        A.CallTo(() => fs.EnumerateFilesRecursive(agentRoot))
            .Returns(["dotnet-developer.md"]);
        A.CallTo(() => fs.ReadAllText(Path.Combine(agentRoot, "dotnet-developer.md")))
            .Returns("---\nname: dotnet-developer\ndescription: builds dotnet\ntags: [csharp]\n---\nbody");

        var console = new TestConsole();
        var sut = new InfoCommand(fs, console);

        var result = sut.Execute(NewContext(),
            new InfoCommand.Settings { AssetId = "dotnet-developer", SourceRepo = sourceRepo });

        result.Should().Be(0);
        console.Output.Should().Contain("dotnet-developer");
        console.Output.Should().Contain("builds dotnet");
    }

    private static void ConfigureEmptyRepo(IFileSystem fs, string sourceRepo)
    {
        A.CallTo(() => fs.DirectoryExists(sourceRepo)).Returns(true);
        A.CallTo(() => fs.DirectoryExists(A<string>.That.StartsWith(sourceRepo + Path.DirectorySeparatorChar)))
            .Returns(false);
        A.CallTo(() => fs.EnumerateFilesRecursive(A<string>._)).Returns([]);
    }

    private static CommandContext NewContext() =>
        new(Array.Empty<string>(), A.Fake<IRemainingArguments>(), "info", null);
}
