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


    [Fact]
    public void Distance_NullArguments_Throws()
    {
        Action actA = () => Levenshtein.Distance(null!, "x");
        Action actB = () => Levenshtein.Distance("x", null!);

        actA.Should().Throw<ArgumentNullException>();
        actB.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SuggestSimilar_NullInput_Throws()
    {
        Action act = () => Levenshtein.SuggestSimilar(null!, new[] { "foo" });

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SuggestSimilar_NullCandidates_Throws()
    {
        Action act = () => Levenshtein.SuggestSimilar("foo", null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SuggestSimilar_FewerCandidatesThanMax_ReturnsAllSorted()
    {
        var result = Levenshtein.SuggestSimilar("foo", new[] { "bar", "foo" }, max: 5);

        result.Should().HaveCount(2);
        result[0].Should().Be("foo");
    }

    [Fact]
    public void SuggestSimilar_TiedDistance_OrderedByOrdinalCandidate()
    {
        var result = Levenshtein.SuggestSimilar("xyz", new[] { "zzz", "yyy", "aaa" }, max: 3);

        result.Should().HaveCount(3);
        result[0].Should().Be("yyy");
    }
}
