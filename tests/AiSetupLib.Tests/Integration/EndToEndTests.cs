using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using AiSetupLib;
using AiSetupLib.Deploy;
using AiSetupLib.Models;
using AiSetupLib.Paths;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetupLib.Tests.Integration;

public class EndToEndTests
{
    private const string RepoRoot = "/repo";

    private static (IDeployService svc, MockFileSystem fs) BuildContainer()
    {
        var fs = new MockFileSystem();
        SeedRepo(fs);

        var services = new ServiceCollection();
        services.AddAiSetup(RepoRoot);
        services.AddSingleton<IFileSystem>(fs);
        services.AddSingleton<IPathProvider>(_ => new PathProvider(home: "/home/me", platform: PlatformKind.Linux));
        var sp = services.BuildServiceProvider();
        return (sp.GetRequiredService<IDeployService>(), fs);
    }

    private static void SeedRepo(MockFileSystem fs)
    {
        fs.AddFile($"{RepoRoot}/instructions/csharp/csharp.instructions.md", new MockFileData("""
            ---
            name: csharp.instructions
            description: C# rules
            type: instruction
            targets: [copilot-cli, claude-code]
            ---
            Use sealed.
            """));
        fs.AddFile($"{RepoRoot}/agents/dotnet-developer.md", new MockFileData("""
            ---
            name: dotnet-developer
            description: .NET dev
            type: agent
            targets: [claude-code]
            ---
            Agent body
            """));
        fs.AddFile($"{RepoRoot}/profiles/dotnet-dev.yaml", new MockFileData("""
            name: dotnet-dev
            description: .NET
            agents: [dotnet-developer]
            instructions: [csharp/csharp.instructions]
            """));
    }

    [Fact]
    public void Deploys_profile_to_claude_code_repo_writing_aggregated_md()
    {
        var (svc, fs) = BuildContainer();

        svc.Deploy(new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, "/dest")
        {
            Profile = "dotnet-dev",
        });

        fs.File.Exists("/dest/CLAUDE.md").Should().BeTrue();
        var md = fs.File.ReadAllText("/dest/CLAUDE.md");
        md.Should().Contain("Use sealed.");
        md.Should().Contain("Agent body");
    }

    [Fact]
    public void Deploys_profile_to_copilot_cli_repo_writing_individual_files()
    {
        var (svc, fs) = BuildContainer();

        svc.Deploy(new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, "/dest")
        {
            Profile = "dotnet-dev",
        });

        fs.File.Exists("/dest/.github/instructions/csharp/csharp.instructions.md").Should().BeTrue();
        fs.File.Exists("/dest/.github/agents/dotnet-developer.md").Should().BeTrue();
    }

    [Fact]
    public void Dry_run_writes_nothing()
    {
        var (svc, fs) = BuildContainer();

        var plan = svc.Deploy(new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, "/dest")
        {
            Profile = "dotnet-dev",
            DryRun = true,
        });

        plan.Actions.Should().NotBeEmpty();
        fs.File.Exists("/dest/CLAUDE.md").Should().BeFalse();
    }
}
