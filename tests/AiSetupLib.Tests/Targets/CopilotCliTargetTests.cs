using AiSetup.Lib.Aggregation;
using AiSetup.Lib.Models;
using AiSetup.Lib.Targets;
using AiSetup.Lib.Targets.Platform;
using FluentAssertions;
using FakeItEasy;
using Xunit;

namespace AiSetup.Lib.Tests.Targets;

public class CopilotCliTargetTests
{
    private static AssetDefinition Asset(string name, AssetType type, string absolutePath)
        => new(
            Name: name,
            Description: string.Empty,
            Type: type,
            Tags: Array.Empty<string>(),
            Targets: Array.Empty<DeployTarget>(),
            ApplyTo: null,
            RelativePath: name,
            AbsolutePath: absolutePath,
            RawContent: $"raw-{name}",
            Body: $"body-{name}",
            Frontmatter: new Dictionary<string, object?>());

    [Fact]
    public void Plan_RepoMode_PlacesInstructionsUnderDotGithub()
    {
        var fs = new InMemoryFileSystem();
        var pathProvider = A.Fake<IPlatformPathProvider>();
        var target = new CopilotCliTarget(fs, pathProvider, new McpConfigMerger());
        var asset = Asset("rules", AssetType.Instruction, "/repo/instructions/rules.md");

        var plan = target.Plan(
            new[] { asset },
            new DeployOptions(
                Target: DeployTarget.CopilotCli,
                Mode: DeployMode.Repo,
                RepoPath: "/dst",
                Profile: null,
                Agents: Array.Empty<string>(),
                Instructions: Array.Empty<string>(),
                Skills: Array.Empty<string>(),
                McpConfigs: Array.Empty<string>(),
                IncludeMcpConfigs: false,
                DryRun: true,
                Force: false));

        plan.Actions.Should().ContainSingle();
        plan.Actions[0].TargetPath.Should().Contain(Path.Combine(".github", "instructions"));
        plan.Actions[0].Kind.Should().Be(DeployActionKind.Create);
    }

    [Fact]
    public void Plan_LocalMode_UsesPathProviderRoot()
    {
        var fs = new InMemoryFileSystem();
        var pathProvider = A.Fake<IPlatformPathProvider>();
        A.CallTo(() => pathProvider.GetLocalRoot(DeployTarget.CopilotCli)).Returns("/local/copilot");
        var target = new CopilotCliTarget(fs, pathProvider, new McpConfigMerger());
        var asset = Asset("agent1", AssetType.Agent, "/repo/agents/agent1.md");

        var plan = target.Plan(
            new[] { asset },
            new DeployOptions(
                Target: DeployTarget.CopilotCli,
                Mode: DeployMode.Local,
                RepoPath: null,
                Profile: null,
                Agents: Array.Empty<string>(),
                Instructions: Array.Empty<string>(),
                Skills: Array.Empty<string>(),
                McpConfigs: Array.Empty<string>(),
                IncludeMcpConfigs: false,
                DryRun: true,
                Force: false));

        plan.Actions[0].TargetPath.Should().StartWith("/local/copilot");
    }

    [Fact]
    public void Plan_SkillsAreCopiedAsDirectory()
    {
        var fs = new InMemoryFileSystem();
        fs.AddFile("/repo/skills/csharp/dotnet-tester/SKILL.md", "x");
        var pathProvider = A.Fake<IPlatformPathProvider>();
        var target = new CopilotCliTarget(fs, pathProvider, new McpConfigMerger());
        var asset = Asset("dotnet-tester", AssetType.Skill, "/repo/skills/csharp/dotnet-tester/SKILL.md");

        var plan = target.Plan(
            new[] { asset },
            new DeployOptions(
                DeployTarget.CopilotCli,
                DeployMode.Repo,
                "/dst",
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                false,
                true,
                false));

        plan.Actions.Should().ContainSingle()
            .Which.IsDirectoryCopy.Should().BeTrue();
    }

    [Fact]
    public void Plan_McpConfig_RequiresIncludeFlag()
    {
        var fs = new InMemoryFileSystem();
        var pathProvider = A.Fake<IPlatformPathProvider>();
        var target = new CopilotCliTarget(fs, pathProvider, new McpConfigMerger());
        var asset = Asset("github", AssetType.McpConfig, "/repo/mcp-configs/github.yaml") with
        {
            RawContent = "command: gh\n",
        };

        var withMcp = target.Plan(new[] { asset }, Options(true));
        var withoutMcp = target.Plan(new[] { asset }, Options(false));

        withMcp.Actions.Should().ContainSingle();
        withoutMcp.Actions.Should().BeEmpty();

        static DeployOptions Options(bool include) =>
            new(
                DeployTarget.CopilotCli,
                DeployMode.Repo,
                "/dst",
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                include,
                true,
                false);
    }
}
