using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using AiSetupLib.Aggregation;
using AiSetupLib.Models;
using AiSetupLib.Paths;
using AiSetupLib.Targets;

namespace AiSetupLib.Tests.Targets;

public class CopilotCliTargetTests
{
    private const string Dest = "/work/target-repo";

    private static AssetDefinition Asset(string name, AssetType type, string body = "body")
        => new(name, "desc", type, [], [DeployTarget.CopilotCli], null, $"/src/{name}", body);

    private static (CopilotCliTarget target, MockFileSystem fs) Make()
    {
        var fs = new MockFileSystem();
        var target = new CopilotCliTarget(fs, new PathProvider(home: "/home/me"), new McpConfigMerger());
        return (target, fs);
    }

    [Fact]
    public void Plan_repo_mode_creates_instruction_file_under_github_instructions()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("csharp/csharp.instructions", AssetType.Instruction, "rules") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);

        plan.Actions.Should().ContainSingle().Which.Should().Match<DeployAction>(a =>
            a.Kind == DeployActionKind.Create
            && a.TargetPath == "/work/target-repo/.github/instructions/csharp/csharp.instructions.md");
    }

    [Fact]
    public void Apply_repo_mode_writes_instruction_body_to_disk()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("csharp/csharp.instructions", AssetType.Instruction, "rules go here") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);
        target.Apply(plan, assets, opts);

        var path = "/work/target-repo/.github/instructions/csharp/csharp.instructions.md";
        fs.File.Exists(path).Should().BeTrue();
        fs.File.ReadAllText(path).Should().Contain("rules go here");
    }

    [Fact]
    public void Apply_repo_mode_writes_skill_under_skill_md_in_subfolder()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("csharp/dotnet-tester", AssetType.Skill, "skill body") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);
        target.Apply(plan, assets, opts);

        var path = "/work/target-repo/.github/skills/csharp/dotnet-tester/SKILL.md";
        fs.File.Exists(path).Should().BeTrue();
    }

    [Fact]
    public void Apply_repo_mode_writes_agent_under_github_agents()
    {
        var (target, fs) = Make();
        var assets = new[] { Asset("dotnet-developer", AssetType.Agent, "agent body") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        fs.File.Exists("/work/target-repo/.github/agents/dotnet-developer.md").Should().BeTrue();
    }

    [Fact]
    public void Apply_repo_mode_merges_mcp_configs_into_vscode_mcp_json()
    {
        var (target, fs) = Make();
        var assets = new[]
        {
            Asset("github", AssetType.McpConfig, "command: gh-mcp"),
            Asset("filesystem", AssetType.McpConfig, "command: fs-mcp"),
        };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        target.Apply(target.Plan(assets, opts), assets, opts);

        var mcpPath = "/work/target-repo/.vscode/mcp.json";
        fs.File.Exists(mcpPath).Should().BeTrue();
        var json = JsonDocument.Parse(fs.File.ReadAllText(mcpPath));
        json.RootElement.GetProperty("servers").GetProperty("github").GetProperty("command")
            .GetString().Should().Be("gh-mcp");
    }

    [Fact]
    public void Plan_marks_existing_target_as_overwrite_when_force_false()
    {
        var (target, fs) = Make();
        var existing = "/work/target-repo/.github/instructions/x.md";
        fs.AddFile(existing, new MockFileData("old content"));

        var assets = new[] { Asset("x", AssetType.Instruction, "new") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Repo, Dest);

        var plan = target.Plan(assets, opts);

        plan.Actions.Should().ContainSingle()
            .Which.Kind.Should().Be(DeployActionKind.Overwrite);
    }

    [Fact]
    public void Plan_local_mode_uses_local_root_from_path_provider()
    {
        var fs = new MockFileSystem();
        var target = new CopilotCliTarget(
            fs,
            new PathProvider(home: "/home/me", platform: PlatformKind.Linux),
            new McpConfigMerger());
        var assets = new[] { Asset("x", AssetType.Instruction, "body") };
        var opts = new DeployOptions(DeployTarget.CopilotCli, DeployMode.Local, "");

        var plan = target.Plan(assets, opts);

        plan.Actions[0].TargetPath.Should()
            .Be("/home/me/.config/github-copilot/instructions/x.md");
    }
}
