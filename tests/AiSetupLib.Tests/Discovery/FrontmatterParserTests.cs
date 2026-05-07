using AiSetup.Lib.Discovery;
using AiSetup.Lib.Exceptions;
using FluentAssertions;
using Xunit;

namespace AiSetup.Lib.Tests.Discovery;

public class FrontmatterParserTests
{
    private readonly FrontmatterParser _sut = new();

    [Fact]
    public void Parse_WithValidFrontmatter_ReturnsMappedValues()
    {
        var raw = """
                  ---
                  name: dotnet-tester
                  description: Write .NET unit tests
                  tags: [csharp, testing]
                  targets: [copilot-cli, claude-code]
                  ---
                  Body content here.
                  """;

        var result = _sut.Parse(raw, "test.md");

        result.HasFrontmatter.Should().BeTrue();
        result.Frontmatter["name"].Should().Be("dotnet-tester");
        result.Frontmatter["description"].Should().Be("Write .NET unit tests");
        result.Body.Should().Be("Body content here.");
    }

    [Fact]
    public void Parse_WithoutFrontmatter_ReturnsBodyOnly()
    {
        var raw = "Just plain markdown body.";

        var result = _sut.Parse(raw, "test.md");

        result.HasFrontmatter.Should().BeFalse();
        result.Body.Should().Be(raw);
        result.Frontmatter.Should().BeEmpty();
    }

    [Fact]
    public void Parse_WithMissingClosingDelimiter_Throws()
    {
        var raw = "---\nname: foo\nbody never closes";

        var act = () => _sut.Parse(raw, "broken.md");

        act.Should().Throw<InvalidFrontmatterException>()
            .Which.FilePath.Should().Be("broken.md");
    }

    [Fact]
    public void Parse_FrontmatterKeyLookup_IsCaseInsensitive()
    {
        var raw = """
                  ---
                  Name: alpha
                  ---
                  body
                  """;

        var result = _sut.Parse(raw, "x.md");

        result.Frontmatter["name"].Should().Be("alpha");
        result.Frontmatter["NAME"].Should().Be("alpha");
    }
}
