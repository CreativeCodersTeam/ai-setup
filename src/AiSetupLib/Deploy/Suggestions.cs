namespace AiSetupLib.Deploy;

public static class Suggestions
{
    public static IReadOnlyList<string> Closest(string query, IEnumerable<string> candidates, int max = 3)
    {
        var threshold = Math.Max(2, query.Length / 2);
        return candidates
            .Select(c => (Name: c, Distance: Levenshtein(query, c)))
            .Where(t => t.Distance <= threshold)
            .OrderBy(t => t.Distance)
            .Take(max)
            .Select(t => t.Name)
            .ToList();
    }

    private static int Levenshtein(string a, string b)
    {
        if (a.Length == 0) return b.Length;
        if (b.Length == 0) return a.Length;
        var prev = new int[b.Length + 1];
        var curr = new int[b.Length + 1];
        for (var j = 0; j <= b.Length; j++) prev[j] = j;
        for (var i = 1; i <= a.Length; i++)
        {
            curr[0] = i;
            for (var j = 1; j <= b.Length; j++)
            {
                var cost = a[i - 1] == b[j - 1] ? 0 : 1;
                curr[j] = Math.Min(Math.Min(curr[j - 1] + 1, prev[j] + 1), prev[j - 1] + cost);
            }
            (prev, curr) = (curr, prev);
        }
        return prev[b.Length];
    }
}
