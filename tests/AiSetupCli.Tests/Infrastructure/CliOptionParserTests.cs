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
}
