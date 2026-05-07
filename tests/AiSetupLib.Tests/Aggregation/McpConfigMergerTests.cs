using AiSetup.Aggregation;
using AiSetup.Exceptions;
using AiSetup.Models;

namespace AiSetup.Tests.Aggregation;

public sealed class McpConfigMergerTests
{
    [Fact]
    public void Merge_NewServerIntoEmptyJson_ProducesValidStructure()
    {
        // Arrange
        var sut = new McpConfigMerger();

        // Act
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

        // Assert
        json.Should().Contain("\"mcpServers\"");
        json.Should().Contain("\"github\"");
        json.Should().Contain("\"command\": \"npx\"");
        json.Should().Contain("\"GITHUB_TOKEN\": \"tok\"");
    }

    [Fact]
    public void Merge_WithExistingUnrelatedKeys_PreservesThem()
    {
        // Arrange
        var sut = new McpConfigMerger();
        const string existing = """{ "permissions": {"allow":["x"]}, "mcpServers": {"old":{"command":"true"}} }""";

        // Act
        var json = sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.ClaudeCode,
            existing,
            overwriteOnConflict: false);

        // Assert
        json.Should().Contain("\"permissions\"");
        json.Should().Contain("\"old\"");
        json.Should().Contain("\"github\"");
    }

    [Fact]
    public void Merge_OnConflictWithoutForce_ThrowsAiSetupException()
    {
        // Arrange
        var sut = new McpConfigMerger();
        const string existing = """{ "mcpServers": { "github": {"command":"old"} } }""";

        // Act
        Action act = () => sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.ClaudeCode,
            existing,
            overwriteOnConflict: false);

        // Assert
        act.Should().Throw<AiSetupException>().WithMessage("*github*");
    }

    [Fact]
    public void Merge_WithInvalidExistingJson_ThrowsAiSetupException()
    {
        // Arrange
        var sut = new McpConfigMerger();

        // Act
        Action act = () => sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.ClaudeCode,
            existingJson: "{ this is : not json",
            overwriteOnConflict: false);

        // Assert
        act.Should().Throw<AiSetupException>().WithMessage("*not valid JSON*");
    }

    [Fact]
    public void Merge_WithNonMcpAsset_ThrowsAiSetupException()
    {
        // Arrange
        var sut = new McpConfigMerger();
        var notMcp = new AssetDefinition(
            "agent", AssetType.Agent, "agent", string.Empty, [], [],
            "/agent", null, new Dictionary<string, object?>(), "name: x\n");

        // Act
        Action act = () => sut.Merge(
            new[] { notMcp },
            McpServersKey.ClaudeCode,
            existingJson: null,
            overwriteOnConflict: false);

        // Assert
        act.Should().Throw<AiSetupException>().WithMessage("*not an MCP config*");
    }

    [Fact]
    public void Merge_WithVsCodeKey_UsesServersField()
    {
        // Arrange
        var sut = new McpConfigMerger();

        // Act
        var json = sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.CopilotCli,
            existingJson: null,
            overwriteOnConflict: false);

        // Assert
        json.Should().Contain("\"servers\"");
        json.Should().NotContain("\"mcpServers\"");
    }

    [Fact]
    public void Merge_WithEmptyMcpBody_ThrowsAiSetupException()
    {
        // Arrange
        var sut = new McpConfigMerger();

        // Act
        Action act = () => sut.Merge(
            new[] { NewMcp("github", string.Empty) },
            McpServersKey.ClaudeCode,
            existingJson: null,
            overwriteOnConflict: false);

        // Assert
        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Merge_OnConflictWithForce_OverwritesExistingServer()
    {
        // Arrange
        var sut = new McpConfigMerger();
        const string existing = """{ "mcpServers": { "github": {"command":"old"} } }""";

        // Act
        var json = sut.Merge(
            new[] { NewMcp("github", "name: github\ncommand: npx\n") },
            McpServersKey.ClaudeCode,
            existing,
            overwriteOnConflict: true);

        // Assert
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
