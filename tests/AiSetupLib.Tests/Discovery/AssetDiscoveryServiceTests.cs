using System.IO.Abstractions.TestingHelpers;
using AiSetupLib.Discovery;
using AiSetupLib.Models;

namespace AiSetupLib.Tests.Discovery;

public class AssetDiscoveryServiceTests
{
    private const string RepoRoot = "/repo";

    private static AssetDiscoveryService MakeService(MockFileSystem fs)
        => new(fs, new FrontmatterParser());

    [Fact]
    public void Discovers_instruction_under_instructions_dir()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/instructions/csharp/csharp.instructions.md",
            new MockFileData("""
            ---
            name: csharp.instructions
            description: C# guidelines
            type: instruction
            tags: [csharp]
            targets: [copilot-cli, claude-code]
            ---
            # C# rules
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets.Should().ContainSingle()
            .Which.Should().Match<AssetDefinition>(a =>
                a.Name == "csharp/csharp.instructions"
                && a.Type == AssetType.Instruction
                && a.Body.StartsWith("# C# rules"));
    }

    [Fact]
    public void Discovers_skill_via_SKILL_md_in_subfolder()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/skills/csharp/dotnet-tester/SKILL.md",
            new MockFileData("""
            ---
            name: dotnet-tester
            description: Run tests
            type: skill
            tags: [csharp, testing]
            targets: [copilot-cli]
            ---
            Skill body
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets.Should().ContainSingle()
            .Which.Name.Should().Be("csharp/dotnet-tester");
        assets[0].Type.Should().Be(AssetType.Skill);
    }

    [Fact]
    public void Discovers_agent_under_agents_dir()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/agents/dotnet-developer.md",
            new MockFileData("""
            ---
            name: dotnet-developer
            description: .NET dev
            type: agent
            tags: []
            targets: [claude-code]
            ---
            Agent
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets[0].Name.Should().Be("dotnet-developer");
        assets[0].Type.Should().Be(AssetType.Agent);
    }

    [Fact]
    public void Discovers_mcp_config_under_mcp_configs_dir()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/mcp-configs/github.yaml",
            new MockFileData("""
            ---
            name: github
            description: GitHub MCP server
            type: mcp-config
            tags: [github]
            targets: [copilot-cli, claude-code]
            ---
            command: github-mcp
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets[0].Name.Should().Be("github");
        assets[0].Type.Should().Be(AssetType.McpConfig);
    }

    [Fact]
    public void Skips_files_with_invalid_frontmatter()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/instructions/bad.md",
            new MockFileData("no frontmatter at all"));
        fs.AddFile($"{RepoRoot}/instructions/good.md",
            new MockFileData("""
            ---
            name: good
            description: ok
            type: instruction
            ---
            body
            """));

        var assets = MakeService(fs).Discover(RepoRoot);

        assets.Should().ContainSingle().Which.Name.Should().Be("good");
    }

    [Fact]
    public void Returns_empty_when_directories_missing()
    {
        var fs = new MockFileSystem();

        var assets = MakeService(fs).Discover(RepoRoot);

        assets.Should().BeEmpty();
    }

    [Fact]
    public void Parses_targets_from_frontmatter()
    {
        var fs = new MockFileSystem();
        fs.AddFile($"{RepoRoot}/instructions/x.md",
            new MockFileData("""
            ---
            name: x
            description: x
            type: instruction
            targets: [copilot-cli, claude-code]
            ---
            """));

        var asset = MakeService(fs).Discover(RepoRoot)[0];

        asset.Targets.Should().BeEquivalentTo(
            [DeployTarget.CopilotCli, DeployTarget.ClaudeCode]);
    }
}
