using AiSetup.Aggregation;
using AiSetup.Exceptions;
using AiSetup.Models;

namespace AiSetup.Tests.Aggregation;

public sealed class McpConfigMergerTests
{
    [Fact]
    public void Merge_NewServer_IntoEmptyJson_ProducesValidStructure()
    {
        var sut = new McpConfigMerger();

        var json = sut.Merge(
            new[]
            {
                NewMcp("github", """
                                  name: github
                                  type: stdio
                                  command: npx
                                  args: ["-y", "server-github"]
                                  env:
                                    GITHUB_TOKEN: tok
                                  """)
            },
            McpServersKey.ClaudeCode,
            existingJson: null,
            overwriteOnConflict: false);

        json.Should().Contain("\"mcpServers\"");
        json.Should().Contain("\"github\"");
        json.Should().Contain("\"command\": \"npx\"");
        json.Should().Contain("\"GITHUB_TOKEN\": \"tok\"");
    }

    [Fact]
    public void Merge_PreservesExistingUnrelatedKeys()
    {
        var sut = new McpConfigMerger();

        const string existing = """{ "permissions": {"allow":["x"]}, "mcpServers": {"old":{"command":"true"}} }""";

        var json = sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.ClaudeCode,
            existing,
            overwriteOnConflict: false);

        json.Should().Contain("\"permissions\"");
        json.Should().Contain("\"old\"");
        json.Should().Contain("\"github\"");
    }

    [Fact]
    public void Merge_ConflictWithoutForce_Throws()
    {
        var sut = new McpConfigMerger();

        const string existing = """{ "mcpServers": { "github": {"command":"old"} } }""";

        Action act = () => sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.ClaudeCode,
            existing,
            overwriteOnConflict: false);

        act.Should().Throw<AiSetupException>().WithMessage("*github*");
    }

    [Fact]
    public void Merge_InvalidExistingJson_ThrowsAiSetupException()
    {
        var sut = new McpConfigMerger();

        Action act = () => sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.ClaudeCode,
            existingJson: "{ this is : not json",
            overwriteOnConflict: false);

        act.Should().Throw<AiSetupException>().WithMessage("*not valid JSON*");
    }

    [Fact]
    public void Merge_NonMcpAsset_Throws()
    {
        var sut = new McpConfigMerger();

        var notMcp = new AssetDefinition(
            "agent", AssetType.Agent, "agent", string.Empty, [], [],
            "/agent", null, new Dictionary<string, object?>(), "name: x\n");

        Action act = () => sut.Merge(
            new[] { notMcp },
            McpServersKey.ClaudeCode,
            existingJson: null,
            overwriteOnConflict: false);

        act.Should().Throw<AiSetupException>().WithMessage("*not an MCP config*");
    }

    [Fact]
    public void Merge_VsCodeKey_UsesServersField()
    {
        var sut = new McpConfigMerger();

        var json = sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.CopilotCli,
            existingJson: null,
            overwriteOnConflict: false);

        json.Should().Contain("\"servers\"");
        json.Should().NotContain("\"mcpServers\"");
    }

    [Fact]
    public void Merge_EmptyMcpBody_Throws()
    {
        var sut = new McpConfigMerger();

        Action act = () => sut.Merge(
            new[] { NewMcp("github", string.Empty) },
            McpServersKey.ClaudeCode,
            existingJson: null,
            overwriteOnConflict: false);

        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Merge_ConflictWithForce_Overwrites()
    {
        var sut = new McpConfigMerger();

        const string existing = """{ "mcpServers": { "github": {"command":"old"} } }""";

        var json = sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.ClaudeCode,
            existing,
            overwriteOnConflict: true);

        json.Should().Contain("\"command\": \"npx\"");
        json.Should().NotContain("old");
    }

    private static AssetDefinition NewMcp(string id, string yaml) => new(
        Id: id,
        Type: AssetType.McpConfig,
        Name: id,
        Description: string.Empty,
        Tags: [],
        Targets: [],
        SourcePath: "/" + id,
        ApplyTo: null,
        Frontmatter: new Dictionary<string, object?>(),
        Body: yaml);
}
