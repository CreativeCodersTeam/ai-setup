using AiSetup.Cli.Infrastructure;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class CliOptionParserTests
{
    [Fact]
    public void Normalize_NullInput_ReturnsEmpty()
    {
        // Act
        var result = CliOptionParser.Normalize(null);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Normalize_WithCommaSeparatedAndDuplicates_SplitsTrimsAndDeduplicates()
    {
        // Arrange
        var input = new[] { "a,b", "c", "a", "  d  " };

        // Act
        var result = CliOptionParser.Normalize(input);

        // Assert
        result.Should().BeEquivalentTo(new[] { "a", "b", "c", "d" }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Normalize_WithEmptyAndWhitespaceEntries_DropsEmptyEntries()
    {
        // Arrange
        var input = new[] { ",", "", "  ", "x" };

        // Act
        var result = CliOptionParser.Normalize(input);

        // Assert
        result.Should().BeEquivalentTo(new[] { "x" });
    }

    [Fact]
    public void Normalize_WithDuplicates_PreservesFirstOccurrenceOrder()
    {
        // Arrange
        var input = new[] { "z", "a", "m", "a", "z", "m" };

        // Act
        var result = CliOptionParser.Normalize(input);

        // Assert
        result.Should().BeEquivalentTo(new[] { "z", "a", "m" }, options => options.WithStrictOrdering());
    }

    [Fact]
    public void Normalize_WithDifferentCases_TreatsThemAsDistinctValues()
    {
        // Arrange
        var input = new[] { "abc", "ABC", "Abc" };

        // Act
        var result = CliOptionParser.Normalize(input);

        // Assert
        result.Should().HaveCount(3);
    }

    [Theory]
    [InlineData("a, b ,c", new[] { "a", "b", "c" })]
    [InlineData("  trim  ", new[] { "trim" })]
    [InlineData("only-one", new[] { "only-one" })]
    public void Normalize_WithWhitespaceAndCommas_TrimsAndSplitsValues(string raw, string[] expected)
    {
        // Act
        var result = CliOptionParser.Normalize([raw]);

        // Assert
        result.Should().BeEquivalentTo(expected);
    }
}
