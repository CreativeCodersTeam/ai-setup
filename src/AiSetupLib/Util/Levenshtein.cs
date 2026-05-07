using CreativeCoders.Core;

namespace AiSetup.Util;

/// <summary>
/// Levenshtein edit-distance helpers used to suggest similar names for misspelled asset IDs.
/// </summary>
public static class Levenshtein
{
    /// <summary>
    /// Computes the Levenshtein distance between two strings.
    /// </summary>
    /// <param name="a">First string. Must not be null.</param>
    /// <param name="b">Second string. Must not be null.</param>
    /// <returns>The minimum number of single-character edits required to change <paramref name="a"/> into <paramref name="b"/>.</returns>
    public static int Distance(string a, string b)
    {
        Ensure.NotNull(a);
        Ensure.NotNull(b);

        if (a.Length == 0)
        {
            return b.Length;
        }

        if (b.Length == 0)
        {
            return a.Length;
        }

        var previous = new int[b.Length + 1];
        var current = new int[b.Length + 1];

        for (var j = 0; j <= b.Length; j++)
        {
            previous[j] = j;
        }

        for (var i = 1; i <= a.Length; i++)
        {
            current[0] = i;

            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                current[j] = Math.Min(
                    Math.Min(current[j - 1] + 1, previous[j] + 1),
                    previous[j - 1] + cost);
            }

            (previous, current) = (current, previous);
        }

        return previous[b.Length];
    }

    /// <summary>
    /// Returns up to <paramref name="max"/> candidates closest to <paramref name="input"/>, ordered by distance.
    /// </summary>
    /// <param name="input">Query string.</param>
    /// <param name="candidates">Pool of candidate strings.</param>
    /// <param name="max">Maximum number of suggestions to return.</param>
    /// <returns>Suggested strings sorted by ascending edit distance.</returns>
    public static IReadOnlyList<string> SuggestSimilar(string input, IEnumerable<string> candidates, int max = 3)
    {
        Ensure.NotNull(input);
        Ensure.NotNull(candidates);

        if (max <= 0)
        {
            return [];
        }

        return candidates
            .Select(c => (Candidate: c, Distance: Distance(input, c)))
            .OrderBy(t => t.Distance)
            .ThenBy(t => t.Candidate, StringComparer.Ordinal)
            .Take(max)
            .Select(t => t.Candidate)
            .ToArray();
    }
}
