using AiSetup.Lib.Aggregation;
using AiSetup.Lib.Models;
using FluentAssertions;
using Xunit;

namespace AiSetup.Lib.Tests.Aggregation;

public class MarkdownAggregatorTests
{
    private static AssetDefinition Asset(string name, AssetType type, string body, string description = "")
        => new(
            Name: name,
            Description: description,
            Type: type,
            Tags: Array.Empty<string>(),
            Targets: Array.Empty<DeployTarget>(),
            ApplyTo: null,
            RelativePath: name + ".md",
            AbsolutePath: "/repo/" + name + ".md",
            RawContent: body,
            Body: body,
            Frontmatter: new Dictionary<string, object?>());

    [Fact]
    public void Aggregate_OrdersByTypeThenName()
    {
        var assets = new[]
        {
            Asset("z-skill", AssetType.Skill, "skill body"),
            Asset("a-instruction", AssetType.Instruction, "instr body"),
            Asset("m-agent", AssetType.Agent, "agent body"),
        };

        var result = new MarkdownAggregator().Aggregate(assets);

        var instrIndex = result.IndexOf("a-instruction", StringComparison.Ordinal);
        var agentIndex = result.IndexOf("m-agent", StringComparison.Ordinal);
        var skillIndex = result.IndexOf("z-skill", StringComparison.Ordinal);

        instrIndex.Should().BeLessThan(agentIndex);
        agentIndex.Should().BeLessThan(skillIndex);
    }

    [Fact]
    public void Aggregate_IncludesNameAndDescriptionHeader()
    {
        var assets = new[] { Asset("alpha", AssetType.Instruction, "Hello world", "About alpha") };

        var result = new MarkdownAggregator().Aggregate(assets);

        result.Should().Contain("# alpha");
        result.Should().Contain("About alpha");
        result.Should().Contain("Hello world");
    }

    [Fact]
    public void Aggregate_SeparatesItemsWithRule()
    {
        var assets = new[]
        {
            Asset("a", AssetType.Instruction, "first"),
            Asset("b", AssetType.Instruction, "second"),
        };

        var result = new MarkdownAggregator().Aggregate(assets);

        result.Should().Contain("---");
    }
}
