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
}
