using CreativeCoders.Core;

namespace AiSetup.Lib.Util;

/// <summary>
/// Provides similarity-based suggestions for misspelled asset names using
/// Levenshtein edit distance.
/// </summary>
public static class LevenshteinSuggester
{
    /// <summary>
    /// Returns up to <paramref name="maxResults"/> candidates from
    /// <paramref name="candidates"/> that are closest to <paramref name="needle"/>.
    /// </summary>
    /// <param name="needle">The query string.</param>
    /// <param name="candidates">Known names to score against.</param>
    /// <param name="maxResults">Maximum number of suggestions to return.</param>
    /// <param name="maxDistance">Maximum allowed edit distance for a candidate to be returned.</param>
    public static IReadOnlyList<string> Suggest(
        string needle,
        IEnumerable<string> candidates,
        int maxResults = 3,
        int maxDistance = 5)
    {
        Ensure.NotNull(needle, nameof(needle));
        Ensure.NotNull(candidates, nameof(candidates));

        return candidates
            .Select(c => (Name: c, Distance: Distance(needle, c)))
            .Where(t => t.Distance <= maxDistance)
            .OrderBy(t => t.Distance)
            .ThenBy(t => t.Name, StringComparer.Ordinal)
            .Take(maxResults)
            .Select(t => t.Name)
            .ToArray();
    }

    /// <summary>
    /// Computes the Levenshtein edit distance between two strings.
    /// </summary>
    public static int Distance(string a, string b)
    {
        Ensure.NotNull(a, nameof(a));
        Ensure.NotNull(b, nameof(b));

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
}
