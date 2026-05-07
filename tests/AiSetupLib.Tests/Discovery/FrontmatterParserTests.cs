using AiSetup.Discovery;

namespace AiSetup.Tests.Discovery;

public sealed class FrontmatterParserTests
{
    [Fact]
    public void Parse_NoFrontmatter_ReturnsBodyAsIs()
    {
        const string content = "# Heading\n\nbody";

        var result = FrontmatterParser.Parse(content);

        result.Values.Should().BeEmpty();
        result.Body.Should().Be(content);
        result.Warning.Should().BeNull();
    }

    [Fact]
    public void Parse_WithFrontmatter_ReturnsValuesAndStrippedBody()
    {
        const string content = "---\nname: dotnet-tester\ndescription: \"test stuff\"\ntags: [csharp, testing]\ntargets: [copilot-cli, claude-code]\n---\n# Body\n\ncontent";

        var result = FrontmatterParser.Parse(content);

        result.Values.GetString("name").Should().Be("dotnet-tester");
        result.Values.GetString("description").Should().Be("test stuff");
        result.Values.GetStringList("tags").Should().BeEquivalentTo(new[] { "csharp", "testing" });
        result.Values.GetTargets().Should().BeEquivalentTo(new[]
        {
            AiSetup.Models.DeployTarget.CopilotCli,
            AiSetup.Models.DeployTarget.ClaudeCode
        });
        result.Body.Should().StartWith("# Body");
        result.Warning.Should().BeNull();
    }

    [Fact]
    public void Parse_MalformedYaml_ReturnsWarningButRetainsBody()
    {
        const string content = "---\nname: : :\ntags [oops\n---\nthe body";

        var result = FrontmatterParser.Parse(content);

        result.Warning.Should().NotBeNull();
        result.Body.Should().Be("the body");
        result.Values.Should().BeEmpty();
    }

    [Fact]
    public void Parse_MissingClosingMarker_TreatsContentAsBody()
    {
        const string content = "---\nname: foo\nstill no close";

        var result = FrontmatterParser.Parse(content);

        result.Values.Should().BeEmpty();
        result.Body.Should().Be(content);
    }

    [Fact]
    public void Parse_CrlfLineEndings_ParsesFrontmatter()
    {
        const string content = "---\r\nname: foo\r\ndescription: bar\r\n---\r\nbody line";

        var result = FrontmatterParser.Parse(content);

        result.Warning.Should().BeNull();
        result.Values.GetString("name").Should().Be("foo");
        result.Values.GetString("description").Should().Be("bar");
        result.Body.Should().Contain("body line");
    }

    [Fact]
    public void Parse_EmptyFrontmatterBlock_ReturnsEmptyValuesAndBody()
    {
        const string content = "---\n\n---\nthe body";

        var result = FrontmatterParser.Parse(content);

        result.Warning.Should().BeNull();
        result.Values.Should().BeEmpty();
        result.Body.Should().Be("the body");
    }

}
