using AiSetupLib.Aggregation;
using AiSetupLib.Models;

namespace AiSetupLib.Tests.Aggregation;

public class McpConfigMergerTests
{
    private static AssetDefinition Mcp(string name, string body)
        => new(name, "d", AssetType.McpConfig, [], [], null, $"/src/{name}.yaml", body);

    [Fact]
    public void Merges_two_mcp_configs_into_a_dictionary_keyed_by_name()
    {
        var merger = new McpConfigMerger();
        var result = merger.Merge([
            Mcp("github", "command: github-mcp\nargs: [--port, '8080']"),
            Mcp("filesystem", "command: fs-mcp"),
        ]);

        result.Should().ContainKeys("github", "filesystem");
        result["github"].Should().BeOfType<Dictionary<string, object?>>();
    }

    [Fact]
    public void Returns_empty_when_no_assets_provided()
    {
        var result = new McpConfigMerger().Merge([]);
        result.Should().BeEmpty();
    }

    [Fact]
    public void Preserves_string_and_list_values_from_yaml()
    {
        var merger = new McpConfigMerger();
        var result = merger.Merge([
            Mcp("github", "command: github-mcp\nargs:\n  - --verbose\n  - --port=8080"),
        ]);

        var github = (Dictionary<string, object?>)result["github"]!;
        github["command"].Should().Be("github-mcp");
        github["args"].Should().BeAssignableTo<IEnumerable<object?>>();
    }

    [Fact]
    public void Skips_assets_with_empty_body()
    {
        var merger = new McpConfigMerger();
        var result = merger.Merge([Mcp("blank", "")]);
        result.Should().NotContainKey("blank");
    }
}
