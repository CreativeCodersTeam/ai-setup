using AiSetup.Lib.Util;
using FluentAssertions;
using Xunit;

namespace AiSetup.Lib.Tests.Util;

public class LevenshteinSuggesterTests
{
    [Fact]
    public void Distance_IdenticalStrings_ReturnsZero()
    {
        LevenshteinSuggester.Distance("kitten", "kitten").Should().Be(0);
    }

    [Fact]
    public void Distance_KittenSitting_ReturnsThree()
    {
        LevenshteinSuggester.Distance("kitten", "sitting").Should().Be(3);
    }

    [Fact]
    public void Suggest_OrdersByDistanceAscending()
    {
        var suggestions = LevenshteinSuggester.Suggest(
            "tetser",
            new[] { "tester", "writer", "reader", "tasker" });

        suggestions.Should().StartWith("tester");
    }

    [Fact]
    public void Suggest_LimitsResultsToMaxResults()
    {
        var suggestions = LevenshteinSuggester.Suggest(
            "abc",
            new[] { "abd", "abe", "abf", "abg" },
            maxResults: 2);

        suggestions.Should().HaveCount(2);
    }
}
