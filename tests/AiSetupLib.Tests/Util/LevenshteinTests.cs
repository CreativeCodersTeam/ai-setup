using AiSetup.Util;

namespace AiSetup.Tests.Util;

public sealed class LevenshteinTests
{
    [Theory]
    [InlineData("", "", 0)]
    [InlineData("abc", "", 3)]
    [InlineData("", "abc", 3)]
    [InlineData("kitten", "sitting", 3)]
    [InlineData("flaw", "lawn", 2)]
    [InlineData("dotnet-tester", "dotnet-tester", 0)]
    public void Distance_ReturnsExpected(string a, string b, int expected)
    {
        Levenshtein.Distance(a, b).Should().Be(expected);
    }

    [Fact]
    public void SuggestSimilar_ReturnsClosestCandidatesOrdered()
    {
        var candidates = new[]
        {
            "csharp/dotnet-tester",
            "csharp/dotnet-sdk-builder",
            "csharp/csharp-docs",
            "general/create-readme"
        };

        var suggestions = Levenshtein.SuggestSimilar("csharp/dotnet-testr", candidates, max: 2);

        suggestions.Should().HaveCount(2);
        suggestions[0].Should().Be("csharp/dotnet-tester");
    }

    [Fact]
    public void SuggestSimilar_WithMaxZero_ReturnsEmpty()
    {
        Levenshtein.SuggestSimilar("foo", new[] { "foo", "bar" }, max: 0).Should().BeEmpty();
    }
}
