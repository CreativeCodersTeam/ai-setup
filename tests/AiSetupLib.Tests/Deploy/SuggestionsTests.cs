using AiSetupLib.Deploy;

namespace AiSetupLib.Tests.Deploy;

public class SuggestionsTests
{
    [Fact]
    public void Returns_closest_matches_within_distance_threshold()
    {
        string[] candidates = ["dotnet-tester", "dotnet-sdk-builder", "ef-core", "csharp-docs"];

        var hits = Suggestions.Closest("dotnet-test", candidates, max: 3);

        hits[0].Should().Be("dotnet-tester");
    }

    [Fact]
    public void Returns_empty_when_nothing_within_threshold()
    {
        var hits = Suggestions.Closest("zzzzz", ["abc", "def"], max: 3);
        hits.Should().BeEmpty();
    }

    [Fact]
    public void Limits_results_to_max()
    {
        string[] candidates = ["aa", "ab", "ac", "ad", "ae"];
        var hits = Suggestions.Closest("a", candidates, max: 2);
        hits.Count.Should().BeLessThanOrEqualTo(2);
    }
}
