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
