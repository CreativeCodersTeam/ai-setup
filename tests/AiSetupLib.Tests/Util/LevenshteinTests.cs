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
    public void Distance_WithVariousStrings_ReturnsExpectedDistance(string a, string b, int expected)
    {
        // Act
        var result = Levenshtein.Distance(a, b);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public void SuggestSimilar_WithCandidates_ReturnsClosestCandidatesOrdered()
    {
        // Arrange
        var candidates = new[]
        {
            "csharp/dotnet-tester",
            "csharp/dotnet-sdk-builder",
            "csharp/csharp-docs",
            "general/create-readme"
        };

        // Act
        var suggestions = Levenshtein.SuggestSimilar("csharp/dotnet-testr", candidates, max: 2);

        // Assert
        suggestions.Should().HaveCount(2);
        suggestions[0].Should().Be("csharp/dotnet-tester");
    }

    [Fact]
    public void SuggestSimilar_WithMaxZero_ReturnsEmpty()
    {
        // Act
        var result = Levenshtein.SuggestSimilar("foo", new[] { "foo", "bar" }, max: 0);

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void Distance_WithNullArguments_ThrowsArgumentNullException()
    {
        // Act
        Action actA = () => Levenshtein.Distance(null!, "x");
        Action actB = () => Levenshtein.Distance("x", null!);

        // Assert
        actA.Should().Throw<ArgumentNullException>();
        actB.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SuggestSimilar_WithNullInput_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => Levenshtein.SuggestSimilar(null!, new[] { "foo" });

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SuggestSimilar_WithNullCandidates_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => Levenshtein.SuggestSimilar("foo", null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SuggestSimilar_WithFewerCandidatesThanMax_ReturnsAllSortedByDistance()
    {
        // Act
        var result = Levenshtein.SuggestSimilar("foo", new[] { "bar", "foo" }, max: 5);

        // Assert
        result.Should().HaveCount(2);
        result[0].Should().Be("foo");
    }

    [Fact]
    public void SuggestSimilar_WithTiedDistance_OrdersByOrdinalCandidate()
    {
        // Act
        var result = Levenshtein.SuggestSimilar("xyz", new[] { "zzz", "yyy", "aaa" }, max: 3);

        // Assert
        result.Should().HaveCount(3);
        result[0].Should().Be("yyy");
    }
}
