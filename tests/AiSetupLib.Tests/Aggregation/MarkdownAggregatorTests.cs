using AiSetup.Aggregation;
using AiSetup.Models;

namespace AiSetup.Tests.Aggregation;

public sealed class MarkdownAggregatorTests
{
    [Fact]
    public void Aggregate_OrdersInstructionsBeforeAgentsAndIncludesBanner()
    {
        var sut = new MarkdownAggregator();

        var output = sut.Aggregate(new[]
        {
            NewAsset(AssetType.Agent, "z-agent", "Agent body"),
            NewAsset(AssetType.Instruction, "csharp/csharp.instructions", "Instr body")
        });

        var instructionIndex = output.IndexOf("Instr body", StringComparison.Ordinal);
        var agentIndex = output.IndexOf("Agent body", StringComparison.Ordinal);
        instructionIndex.Should().BePositive();
        agentIndex.Should().BeGreaterThan(instructionIndex);
    }

    [Fact]
    public void Aggregate_FiltersOutSkillAndMcpAssets()
    {
        var sut = new MarkdownAggregator();

        var output = sut.Aggregate(new[]
        {
            NewAsset(AssetType.Skill, "csharp/dotnet-tester", "should not appear"),
            NewAsset(AssetType.McpConfig, "github", "also not"),
            NewAsset(AssetType.Instruction, "general/code", "should appear")
        });

        output.Should().Contain("should appear");
        output.Should().NotContain("should not appear");
        output.Should().NotContain("also not");
    }

    [Fact]
    public void Aggregate_RendersApplyToHint()
    {
        var sut = new MarkdownAggregator();

        var asset = new AssetDefinition(
            "x", AssetType.Instruction, "x", "desc", [], [],
            "/x", "**/*.cs", new Dictionary<string, object?>(), "body");

        var output = sut.Aggregate([asset]);

        output.Should().Contain("Applies to: `**/*.cs`");
    }

    [Fact]
    public void Aggregate_PreservesProfileOrderForInstructions()
    {
        var sut = new MarkdownAggregator();

        var output = sut.Aggregate(new[]
        {
            NewAsset(AssetType.Instruction, "b/foo.instructions", "Body B"),
            NewAsset(AssetType.Instruction, "a/bar.instructions", "Body A")
        });

        var bIndex = output.IndexOf("Body B", StringComparison.Ordinal);
        var aIndex = output.IndexOf("Body A", StringComparison.Ordinal);
        bIndex.Should().BePositive();
        aIndex.Should().BeGreaterThan(bIndex);
    }

    [Fact]
    public void Aggregate_PutsGeneralInstructionsFirst()
    {
        var sut = new MarkdownAggregator();

        var output = sut.Aggregate(new[]
        {
            NewAsset(AssetType.Instruction, "csharp/csharp.instructions", "Csharp body"),
            NewAsset(AssetType.Instruction, "general/general.instructions", "General body")
        });

        var generalIndex = output.IndexOf("General body", StringComparison.Ordinal);
        var csharpIndex = output.IndexOf("Csharp body", StringComparison.Ordinal);
        generalIndex.Should().BePositive();
        csharpIndex.Should().BeGreaterThan(generalIndex);
    }

    [Fact]
    public void Aggregate_PreservesOrderAmongMultipleGeneralInstructions()
    {
        var sut = new MarkdownAggregator();

        var output = sut.Aggregate(new[]
        {
            NewAsset(AssetType.Instruction, "general/b", "General B body"),
            NewAsset(AssetType.Instruction, "csharp/x", "Csharp X body"),
            NewAsset(AssetType.Instruction, "general/a", "General A body")
        });

        var generalBIndex = output.IndexOf("General B body", StringComparison.Ordinal);
        var generalAIndex = output.IndexOf("General A body", StringComparison.Ordinal);
        var csharpXIndex = output.IndexOf("Csharp X body", StringComparison.Ordinal);

        generalBIndex.Should().BePositive();
        generalAIndex.Should().BeGreaterThan(generalBIndex);
        csharpXIndex.Should().BeGreaterThan(generalAIndex);
    }

    private static AssetDefinition NewAsset(AssetType type, string id, string body) => new(
        Id: id,
        Type: type,
        Name: id,
        Description: string.Empty,
        Tags: [],
        Targets: [],
        SourcePath: "/" + id,
        ApplyTo: null,
        Frontmatter: new Dictionary<string, object?>(),
        Body: body);
}
