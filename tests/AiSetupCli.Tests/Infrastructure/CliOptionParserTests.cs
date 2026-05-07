using AiSetup.Cli.Infrastructure;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class CliOptionParserTests
{
    [Fact]
    public void Normalize_NullInput_ReturnsEmpty()
    {
        CliOptionParser.Normalize(null).Should().BeEmpty();
    }

    [Fact]
    public void Normalize_SplitsCommaAndDeduplicates()
    {
        var input = new[] { "a,b", "c", "a", "  d  " };

        var result = CliOptionParser.Normalize(input);

        result.Should().BeEquivalentTo(new[] { "a", "b", "c", "d" }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Normalize_DropsEmptyEntries()
    {
        var input = new[] { ",", "", "  ", "x" };
        CliOptionParser.Normalize(input).Should().BeEquivalentTo(new[] { "x" });
    }

    [Fact]
    public void Normalize_PreservesFirstOccurrenceOrder_WhenDeduplicating()
    {
        var input = new[] { "z", "a", "m", "a", "z", "m" };

        var result = CliOptionParser.Normalize(input);

        result.Should().BeEquivalentTo(new[] { "z", "a", "m" }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Normalize_IsCaseSensitive()
    {
        var input = new[] { "abc", "ABC", "Abc" };

        var result = CliOptionParser.Normalize(input);

        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("a, b ,c", new[] { "a", "b", "c" })]
    [InlineData("  trim  ", new[] { "trim" })]
    [InlineData("only-one", new[] { "only-one" })]
    public void Normalize_HandlesWhitespaceAndCommas(string raw, string[] expected)
    {
        CliOptionParser.Normalize([raw]).Should().BeEquivalentTo(expected);
    }
}
