using AiSetup.Lib.Aggregation;
using AiSetup.Lib.Models;
using AiSetup.Lib.Targets;
using AiSetup.Lib.Targets.Platform;
using FluentAssertions;
using FakeItEasy;
using Xunit;

namespace AiSetup.Lib.Tests.Targets;

public class ClaudeCodeTargetTests
{
    private static AssetDefinition Asset(string name, AssetType type, string body)
        => new(
            Name: name,
            Description: $"desc-{name}",
            Type: type,
            Tags: Array.Empty<string>(),
            Targets: Array.Empty<DeployTarget>(),
            ApplyTo: null,
            RelativePath: name,
            AbsolutePath: "/repo/" + name + ".md",
            RawContent: body,
            Body: body,
            Frontmatter: new Dictionary<string, object?>());

    [Fact]
    public void Plan_AggregatesInstructionsAndAgentsIntoClaudeMd()
    {
        var fs = new InMemoryFileSystem();
        var target = new ClaudeCodeTarget(
            fs,
            A.Fake<IPlatformPathProvider>(),
            new MarkdownAggregator(),
            new McpConfigMerger());

        var assets = new[]
        {
            Asset("instr1", AssetType.Instruction, "instr-body"),
            Asset("agent1", AssetType.Agent, "agent-body"),
        };

        var plan = target.Plan(
            assets,
            new DeployOptions(
                DeployTarget.ClaudeCode,
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

        plan.Actions.Should().ContainSingle();
        var action = plan.Actions[0];
        action.TargetPath.Should().EndWith("CLAUDE.md");
        action.Content.Should().Contain("instr-body");
        action.Content.Should().Contain("agent-body");
    }

    [Fact]
    public void Plan_McpConfigsLandInSettingsJson()
    {
        var fs = new InMemoryFileSystem();
        var target = new ClaudeCodeTarget(
            fs,
            A.Fake<IPlatformPathProvider>(),
            new MarkdownAggregator(),
            new McpConfigMerger());

        var asset = Asset("github", AssetType.McpConfig, "command: gh\n");

        var plan = target.Plan(
            new[] { asset },
            new DeployOptions(
                DeployTarget.ClaudeCode,
                DeployMode.Repo,
                "/dst",
                null,
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                Array.Empty<string>(),
                true,
                true,
                false));

        plan.Actions.Should().ContainSingle();
        plan.Actions[0].TargetPath.Should().EndWith(Path.Combine(".claude", "settings.json"));
        plan.Actions[0].Content.Should().Contain("mcpServers");
    }
}
