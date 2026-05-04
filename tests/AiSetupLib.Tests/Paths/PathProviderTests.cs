using AiSetupLib.Models;
using AiSetupLib.Paths;

namespace AiSetupLib.Tests.Paths;

public class PathProviderTests
{
    [Fact]
    public void Resolves_claude_code_local_root_to_supplied_home_subfolder()
    {
        var provider = new PathProvider(home: "/home/me");

        var root = provider.GetLocalRoot(DeployTarget.ClaudeCode);

        root.Should().Be("/home/me/.claude");
    }

    [Fact]
    public void Resolves_copilot_cli_local_root_per_platform()
    {
        var linux = new PathProvider(home: "/home/me", platform: PlatformKind.Linux);
        var mac = new PathProvider(home: "/Users/me", platform: PlatformKind.MacOS);
        var win = new PathProvider(home: @"C:\Users\me", platform: PlatformKind.Windows,
            appData: @"C:\Users\me\AppData\Roaming");

        linux.GetLocalRoot(DeployTarget.CopilotCli)
            .Should().Be("/home/me/.config/github-copilot");
        mac.GetLocalRoot(DeployTarget.CopilotCli)
            .Should().Be("/Users/me/Library/Application Support/github-copilot");
        win.GetLocalRoot(DeployTarget.CopilotCli)
            .Should().Be(@"C:\Users\me\AppData\Roaming\GitHub Copilot CLI");
    }

    [Fact]
    public void Returns_repo_subpaths_for_copilot_cli()
    {
        var provider = new PathProvider(home: "/h");

        provider.GetRepoSubPath(DeployTarget.CopilotCli, AssetType.Instruction)
            .Should().Be(".github/instructions");
        provider.GetRepoSubPath(DeployTarget.CopilotCli, AssetType.Skill)
            .Should().Be(".github/skills");
        provider.GetRepoSubPath(DeployTarget.CopilotCli, AssetType.Agent)
            .Should().Be(".github/agents");
    }

    [Fact]
    public void Returns_aggregated_file_path_for_claude_code()
    {
        var provider = new PathProvider(home: "/h");
        provider.GetClaudeAggregatedFileName().Should().Be("CLAUDE.md");
    }

    [Fact]
    public void Returns_mcp_settings_file_for_each_target()
    {
        var provider = new PathProvider(home: "/h");
        provider.GetMcpSettingsRelativePath(DeployTarget.CopilotCli)
            .Should().Be(".vscode/mcp.json");
        provider.GetMcpSettingsRelativePath(DeployTarget.ClaudeCode)
            .Should().Be(".claude/settings.json");
    }
}
