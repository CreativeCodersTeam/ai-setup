using System.Text.Json.Nodes;
using AiSetup.Lib.Aggregation;
using AiSetup.Lib.Models;
using FluentAssertions;
using Xunit;

namespace AiSetup.Lib.Tests.Aggregation;

public class McpConfigMergerTests
{
    private static AssetDefinition McpAsset(string name, string yaml)
        => new(
            Name: name,
            Description: string.Empty,
            Type: AssetType.McpConfig,
            Tags: Array.Empty<string>(),
            Targets: Array.Empty<DeployTarget>(),
            ApplyTo: null,
            RelativePath: name + ".yaml",
            AbsolutePath: "/repo/" + name + ".yaml",
            RawContent: yaml,
            Body: yaml,
            Frontmatter: new Dictionary<string, object?>());

    [Fact]
    public void Merge_NewConfig_PlacesEntriesUnderRootKey()
    {
        var asset = McpAsset("github", "command: gh-mcp\nargs: [run]\n");

        var result = new McpConfigMerger().Merge(new[] { asset }, existingJson: null, settingsRootKey: "mcpServers");

        var root = JsonNode.Parse(result)!.AsObject();
        root["mcpServers"].Should().NotBeNull();
        root["mcpServers"]!["github"]!["command"]!.GetValue<string>().Should().Be("gh-mcp");
    }

    [Fact]
    public void Merge_PreservesExistingUnrelatedEntries()
    {
        var existing = """{ "mcpServers": { "old": { "command": "x" } }, "other": 1 }""";
        var asset = McpAsset("github", "command: gh-mcp\n");

        var result = new McpConfigMerger().Merge(new[] { asset }, existing, settingsRootKey: "mcpServers");

        var root = JsonNode.Parse(result)!.AsObject();
        root["other"]!.GetValue<int>().Should().Be(1);
        root["mcpServers"]!["old"]!["command"]!.GetValue<string>().Should().Be("x");
        root["mcpServers"]!["github"]!["command"]!.GetValue<string>().Should().Be("gh-mcp");
    }

    [Fact]
    public void Merge_OverwritesEntryWithSameName()
    {
        var existing = """{ "mcpServers": { "github": { "command": "old" } } }""";
        var asset = McpAsset("github", "command: new\n");

        var result = new McpConfigMerger().Merge(new[] { asset }, existing, settingsRootKey: "mcpServers");

        var root = JsonNode.Parse(result)!.AsObject();
        root["mcpServers"]!["github"]!["command"]!.GetValue<string>().Should().Be("new");
    }

    [Fact]
    public void Merge_WithoutRootKey_ProducesFlatObject()
    {
        var asset = McpAsset("github", "command: gh-mcp\n");

        var result = new McpConfigMerger().Merge(new[] { asset }, existingJson: null, settingsRootKey: null);

        var root = JsonNode.Parse(result)!.AsObject();
        root["github"]!["command"]!.GetValue<string>().Should().Be("gh-mcp");
    }
}
