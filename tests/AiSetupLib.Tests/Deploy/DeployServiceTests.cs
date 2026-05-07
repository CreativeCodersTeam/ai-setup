using AiSetup.Lib.Aggregation;
using AiSetup.Lib.Deploy;
using AiSetup.Lib.Discovery;
using AiSetup.Lib.Exceptions;
using AiSetup.Lib.Models;
using AiSetup.Lib.Profiles;
using AiSetup.Lib.Targets;
using AiSetup.Lib.Targets.Platform;
using FluentAssertions;
using FakeItEasy;
using Xunit;

namespace AiSetup.Lib.Tests.Deploy;

public class DeployServiceTests
{
    private const string SourceRoot = "/repo";
    private const string DestRoot = "/dst";

    private static (DeployService Service, InMemoryFileSystem Fs) BuildSut(InMemoryFileSystem fs)
    {
        var parser = new FrontmatterParser();
        var discovery = new AssetDiscoveryService(fs, parser);
        var profileResolver = new ProfileResolver(fs);
        var pathProvider = A.Fake<IPlatformPathProvider>();
        var copilot = new CopilotCliTarget(fs, pathProvider, new McpConfigMerger());
        var claude = new ClaudeCodeTarget(fs, pathProvider, new MarkdownAggregator(), new McpConfigMerger());
        var registry = new TargetRegistry(new IDeployTarget[] { copilot, claude });
        var sut = new DeployService(discovery, profileResolver, registry, fs);

        return (sut, fs);
    }

    private static InMemoryFileSystem RepoWithProfile()
    {
        var fs = new InMemoryFileSystem();
        fs.AddFile($"{SourceRoot}/instructions/csharp/csharp.instructions.md", """
                                                                                ---
                                                                                name: csharp
                                                                                description: rules
                                                                                ---
                                                                                csharp body
                                                                                """);
        fs.AddFile($"{SourceRoot}/profiles/dotnet-dev.yaml", """
                                                              name: dotnet-dev
                                                              instructions:
                                                                - csharp/csharp.instructions
                                                              """);

        return fs;
    }

    [Fact]
    public void Deploy_DryRun_DoesNotWriteFiles()
    {
        var fs = RepoWithProfile();
        var (sut, _) = BuildSut(fs);

        var options = new DeployOptions(
            DeployTarget.ClaudeCode,
            DeployMode.Repo,
            DestRoot,
            "dotnet-dev",
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            false,
            DryRun: true,
            Force: false);

        var result = sut.Deploy(SourceRoot, options);

        result.Applied.Should().BeFalse();
        result.Plan.Actions.Should().NotBeEmpty();
        fs.FileExists(Path.Combine(DestRoot, "CLAUDE.md")).Should().BeFalse();
    }

    [Fact]
    public void Deploy_NonDryRun_WritesAggregatedClaudeMd()
    {
        var fs = RepoWithProfile();
        var (sut, _) = BuildSut(fs);

        var options = new DeployOptions(
            DeployTarget.ClaudeCode,
            DeployMode.Repo,
            DestRoot,
            "dotnet-dev",
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            false,
            DryRun: false,
            Force: false);

        var result = sut.Deploy(SourceRoot, options);

        result.Applied.Should().BeTrue();
        var path = Path.Combine(DestRoot, "CLAUDE.md");
        fs.FileExists(path).Should().BeTrue();
        fs.ReadAllText(path).Should().Contain("csharp body");
    }

    [Fact]
    public void Deploy_MissingProfile_ThrowsWithSuggestions()
    {
        var fs = RepoWithProfile();
        var (sut, _) = BuildSut(fs);

        var options = new DeployOptions(
            DeployTarget.ClaudeCode,
            DeployMode.Repo,
            DestRoot,
            "dotnet-deve",
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            false,
            true,
            false);

        var act = () => sut.Deploy(SourceRoot, options);

        act.Should().Throw<MissingAssetException>()
            .Which.Suggestions.Should().Contain("dotnet-dev");
    }

    [Fact]
    public void Deploy_OverwriteRequiresForce()
    {
        var fs = RepoWithProfile();
        var existingPath = Path.Combine(DestRoot, "CLAUDE.md");
        fs.AddFile(existingPath, "previous content");
        var (sut, _) = BuildSut(fs);

        var optionsNoForce = new DeployOptions(
            DeployTarget.ClaudeCode,
            DeployMode.Repo,
            DestRoot,
            "dotnet-dev",
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            Array.Empty<string>(),
            false,
            DryRun: false,
            Force: false);

        sut.Deploy(SourceRoot, optionsNoForce);
        fs.ReadAllText(existingPath).Should().Be("previous content");

        var optionsForce = optionsNoForce with { Force = true };
        sut.Deploy(SourceRoot, optionsForce);
        fs.ReadAllText(existingPath).Should().Contain("csharp body");
    }
}
