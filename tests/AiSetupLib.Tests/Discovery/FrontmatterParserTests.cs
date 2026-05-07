using AiSetup.Discovery;

namespace AiSetup.Tests.Discovery;

public sealed class FrontmatterParserTests
{
    [Fact]
    public void Parse_WithoutFrontmatter_ReturnsBodyAsIs()
    {
        // Arrange
        const string content = "# Heading\n\nbody";

        // Act
        var result = FrontmatterParser.Parse(content);

        // Assert
        result.Values.Should().BeEmpty();
        result.Body.Should().Be(content);
        result.Warning.Should().BeNull();
    }

    [Fact]
    public void Parse_WithValidFrontmatter_ReturnsValuesAndStrippedBody()
    {
        // Arrange
        const string content = "---\nname: dotnet-tester\ndescription: \"test stuff\"\ntags: [csharp, testing]\ntargets: [copilot-cli, claude-code]\n---\n# Body\n\ncontent";

        // Act
        var result = FrontmatterParser.Parse(content);

        // Assert
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
    public void Parse_WithMalformedYaml_ReturnsWarningButRetainsBody()
    {
        // Arrange
        const string content = "---\nname: : :\ntags [oops\n---\nthe body";

        // Act
        var result = FrontmatterParser.Parse(content);

        // Assert
        result.Warning.Should().NotBeNull();
        result.Body.Should().Be("the body");
        result.Values.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithMissingClosingMarker_TreatsContentAsBody()
    {
        // Arrange
        const string content = "---\nname: foo\nstill no close";

        // Act
        var result = FrontmatterParser.Parse(content);

        // Assert
        result.Values.Should().BeEmpty();
        result.Body.Should().Be(content);
    }

    [Fact]
    public void Parse_WithCrlfLineEndings_ParsesFrontmatter()
    {
        // Arrange
        const string content = "---\r\nname: foo\r\ndescription: bar\r\n---\r\nbody line";

        // Act
        var result = FrontmatterParser.Parse(content);

        // Assert
        result.Warning.Should().BeNull();
        result.Values.GetString("name").Should().Be("foo");
        result.Values.GetString("description").Should().Be("bar");
        result.Body.Should().Contain("body line");
    }

    [Fact]
    public void Parse_WithEmptyFrontmatterBlock_ReturnsEmptyValuesAndBody()
    {
        // Arrange
        const string content = "---\n\n---\nthe body";

        // Act
        var result = FrontmatterParser.Parse(content);

        // Assert
        result.Warning.Should().BeNull();
        result.Values.Should().BeEmpty();
        result.Body.Should().Be("the body");
    }
}
