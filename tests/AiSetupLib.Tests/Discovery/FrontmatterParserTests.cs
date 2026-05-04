using AiSetupLib.Discovery;

namespace AiSetupLib.Tests.Discovery;

public class FrontmatterParserTests
{
    private readonly FrontmatterParser _parser = new();

    [Fact]
    public void Parses_frontmatter_and_body()
    {
        var input = """
            ---
            name: dotnet-tester
            description: "Run tests"
            tags: [csharp, testing]
            type: skill
            ---
            # Body

            Some content.
            """;

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeTrue();
        result.Frontmatter["name"].Should().Be("dotnet-tester");
        result.Frontmatter["description"].Should().Be("Run tests");
        result.Frontmatter["type"].Should().Be("skill");
        result.Body.Should().StartWith("# Body");
    }

    [Fact]
    public void Returns_empty_frontmatter_when_no_delimiter()
    {
        var input = "# Just a markdown file\n\nNo frontmatter here.";

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeFalse();
        result.Frontmatter.Should().BeEmpty();
        result.Body.Should().Be(input);
    }

    [Fact]
    public void Returns_empty_when_frontmatter_is_unterminated()
    {
        var input = """
            ---
            name: oops
            no closing fence
            """;

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeFalse();
        result.Body.Should().Be(input);
    }

    [Fact]
    public void Returns_empty_when_yaml_is_invalid()
    {
        var input = """
            ---
            name: [unclosed
            ---
            body
            """;

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeFalse();
    }

    [Fact]
    public void Parses_list_values_as_string_lists()
    {
        var input = """
            ---
            tags: [a, b, c]
            ---
            body
            """;

        var result = _parser.Parse(input);

        var tags = result.Frontmatter["tags"] as IReadOnlyList<string>;
        tags.Should().BeEquivalentTo("a", "b", "c");
    }

    [Fact]
    public void Preserves_CRLF_line_endings_in_body()
    {
        var input = "---\r\nname: x\r\n---\r\nLine one\r\nLine two\r\n";

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeTrue();
        result.Body.Should().Be("Line one\r\nLine two\r\n");
    }

    [Fact]
    public void Preserves_LF_line_endings_in_body()
    {
        var input = "---\nname: x\n---\nLine one\nLine two\n";

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeTrue();
        result.Body.Should().Be("Line one\nLine two\n");
    }

    [Fact]
    public void Empty_input_returns_no_frontmatter_with_empty_body()
    {
        var result = _parser.Parse("");

        result.HasFrontmatter.Should().BeFalse();
        result.Body.Should().Be("");
    }

    [Fact]
    public void Frontmatter_block_with_no_body_returns_empty_body_when_valid()
    {
        var input = "---\nname: x\n---\n";

        var result = _parser.Parse(input);

        result.HasFrontmatter.Should().BeTrue();
        result.Body.Should().Be("");
    }
}
