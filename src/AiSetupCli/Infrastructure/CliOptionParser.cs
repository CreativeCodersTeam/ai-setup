namespace AiSetup.Cli.Infrastructure;

/// <summary>
/// Helpers for normalising CLI list options into deduped <see cref="IReadOnlyList{T}"/>.
/// </summary>
internal static class CliOptionParser
{
    /// <summary>
    /// Splits values that may be either repeated (`--flag a --flag b`) or comma-separated
    /// (`--flag a,b`). Whitespace is trimmed, empty entries are removed.
    /// </summary>
    /// <param name="values">Raw values from the command settings.</param>
    public static IReadOnlyList<string> Normalize(IEnumerable<string>? values)
    {
        if (values is null)
        {
            return [];
        }

        var seen = new HashSet<string>(StringComparer.Ordinal);
        var result = new List<string>();

        foreach (var raw in values)
        {
            if (string.IsNullOrWhiteSpace(raw))
            {
                continue;
            }

            foreach (var part in raw.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                if (seen.Add(part))
                {
                    result.Add(part);
                }
            }
        }

        return result;
    }
}
