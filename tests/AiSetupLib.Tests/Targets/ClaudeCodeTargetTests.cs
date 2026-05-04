using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AiSetupLib.Aggregation;
using AiSetupLib.Models;
using AiSetupLib.Paths;
using AiSetupLib.Targets;

namespace AiSetupLib.Tests.Targets;

public class ClaudeCodeTargetTests
{
    private const string Dest = "/work/target-repo";

    private static AssetDefinition Asset(string name, AssetType type, string body = "body")
        => new(name, "desc", type, [], [DeployTarget.ClaudeCode], null, $"/src/{name}", body);

    private static (ClaudeCodeTarget target, MockFileSystem fs) Make()
    {
        var fs = new MockFileSystem();
        var target = new ClaudeCodeTarget(
            fs,
            new PathProvider(home: "/home/me"),
            new MarkdownAggregator(),
            new McpConfigMerger());
        return (target, fs);
    }

    [Fact]
    public void Apply_repo_mode_aggregates_instructions_and_agents_into_claude_md()
    {
        var (target, fs) = Make();
        var assets = new[]
        {
            Asset("csharp/rules", AssetType.Instruction, "Use sealed."),
            Asset("dotnet-developer", AssetType.Agent, "Agent."),
        };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        var path = "/work/target-repo/CLAUDE.md";
        fs.File.Exists(path).Should().BeTrue();
        var content = fs.File.ReadAllText(path);
        content.Should().Contain("Use sealed.");
        content.Should().Contain("Agent.");
    }

    [Fact]
    public void Apply_repo_mode_writes_skills_under_dot_claude_skills()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("csharp/dotnet-tester", AssetType.Skill, "skill body") };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        fs.File.Exists("/work/target-repo/.claude/skills/csharp/dotnet-tester/SKILL.md")
            .Should().BeTrue();
    }

    [Fact]
    public void Apply_repo_mode_merges_mcp_into_claude_settings_json()
    {
        var (target, fs) = Make();
        var assets = new[]
        {
            Asset("github", AssetType.McpConfig, "command: gh-mcp"),
        };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        var json = JsonDocument.Parse(
            fs.File.ReadAllText("/work/target-repo/.claude/settings.json"));
        json.RootElement.GetProperty("mcpServers").GetProperty("github")
            .GetProperty("command").GetString().Should().Be("gh-mcp");
    }

    [Fact]
    public void Apply_local_mode_writes_claude_md_to_home_dot_claude()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("x", AssetType.Instruction, "rules") };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Local, "");

        target.Apply(target.Plan(assets, opts), assets, opts);

        fs.File.Exists("/home/me/.claude/CLAUDE.md").Should().BeTrue();
    }

    [Fact]
    public void Plan_emits_one_action_per_aggregate_or_per_skill()
    {
        var (target, _) = Make();
        var assets = new[]
        {
            Asset("a", AssetType.Instruction, "a"),
            Asset("b", AssetType.Agent, "b"),
            Asset("c", AssetType.Skill, "c"),
            Asset("d", AssetType.McpConfig, "command: x"),
        };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);

        // 1 aggregated CLAUDE.md (a + b), 1 skill, 1 settings.json
        plan.Actions.Should().HaveCount(3);
    }

    [Fact]
    public void Plan_marks_existing_settings_json_as_overwrite()
    {
        var (target, fs) = Make();
        fs.AddFile("/work/target-repo/.claude/settings.json", new MockFileData("{}"));
        var assets = new[] { Asset("g", AssetType.McpConfig, "command: x") };
        var opts = new DeployOptions(DeployTarget.ClaudeCode, DeployMode.Repo, Dest);

        var action = target.Plan(assets, opts).Actions
            .Single(a => a.TargetPath.EndsWith("settings.json"));
        action.Kind.Should().Be(DeployActionKind.Overwrite);
    }
}
