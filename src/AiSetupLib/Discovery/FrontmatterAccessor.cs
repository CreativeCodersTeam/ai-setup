using AiSetup.Models;

namespace AiSetup.Discovery;

/// <summary>
/// Convenience extensions to read typed values out of a frontmatter dictionary
/// produced by <see cref="FrontmatterParser"/>.
/// </summary>
public static class FrontmatterAccessor
{
    /// <summary>Returns the string value for <paramref name="key"/>, or null if missing or non-string.</summary>
    public static string? GetString(this IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw))
        {
            return null;
        }

        return raw?.ToString();
    }

    /// <summary>Returns a list of strings for <paramref name="key"/>. Returns an empty list when missing.</summary>
    public static IReadOnlyList<string> GetStringList(this IReadOnlyDictionary<string, object?> values, string key)
    {
        if (!values.TryGetValue(key, out var raw) || raw is null)
        {
            return [];
        }

        return raw switch
        {
            IEnumerable<object?> list => list.Where(x => x is not null).Select(x => x!.ToString()!).ToArray(),
            string s => [s],
            _ => []
        };
    }

    /// <summary>Returns deploy targets parsed from the <c>targets</c> frontmatter list. Empty = all targets.</summary>
    public static IReadOnlyList<DeployTarget> GetTargets(this IReadOnlyDictionary<string, object?> values)
    {
        var raw = values.GetStringList("targets");
        var result = new List<DeployTarget>(raw.Count);

        foreach (var token in raw)
        {
            if (TryParseTarget(token, out var target))
            {
                result.Add(target);
            }
        }

        return result;
    }

    private static bool TryParseTarget(string token, out DeployTarget target)
    {
        switch (token.Trim().ToLowerInvariant())
        {
            case "copilot-cli":
            case "copilotcli":
            case "copilot":
                target = DeployTarget.CopilotCli;
                return true;
            case "claude-code":
            case "claudecode":
            case "claude":
                target = DeployTarget.ClaudeCode;
                return true;
            default:
                target = default;
                return false;
        }
    }
}
