using System.Text.RegularExpressions;
using CreativeCoders.Core;
using YamlDotNet.Serialization;

namespace AiSetup.Discovery;

/// <summary>
/// Extracts and parses an optional YAML frontmatter block from a Markdown document.
/// </summary>
public static partial class FrontmatterParser
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().Build();

    /// <summary>
    /// Parses the given Markdown content. Returns a result that always includes the body
    /// (with frontmatter stripped if present). Malformed YAML is reported via
    /// <see cref="FrontmatterParseResult.Warning"/> instead of throwing.
    /// </summary>
    /// <param name="content">Raw file content. Must not be null.</param>
    /// <returns>Parsed result.</returns>
    public static FrontmatterParseResult Parse(string content)
    {
        Ensure.NotNull(content);

        var match = FrontmatterPattern().Match(content);

        if (!match.Success)
        {
            return new FrontmatterParseResult(EmptyValues, content, Warning: null);
        }

        var yaml = match.Groups["fm"].Value;
        var body = match.Groups["body"].Value;

        try
        {
            var raw = Deserializer.Deserialize<Dictionary<object, object?>>(yaml);
            var values = raw is null
                ? EmptyValues
                : raw.ToDictionary(
                    kvp => kvp.Key.ToString() ?? string.Empty,
                    kvp => Normalize(kvp.Value));
            return new FrontmatterParseResult(values, body, Warning: null);
        }
        catch (Exception ex)
        {
            return new FrontmatterParseResult(EmptyValues, body, Warning: $"Frontmatter parse error: {ex.Message}");
        }
    }

    private static readonly IReadOnlyDictionary<string, object?> EmptyValues =
        new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);

    private static object? Normalize(object? value)
    {
        return value switch
        {
            null => null,
            string s => s,
            IDictionary<object, object?> dict => dict.ToDictionary(
                kvp => kvp.Key.ToString() ?? string.Empty,
                kvp => Normalize(kvp.Value)),
            IEnumerable<object?> list => list.Select(Normalize).ToList(),
            _ => value
        };
    }

    [GeneratedRegex(@"\A---\r?\n(?<fm>.*?)\r?\n---\r?\n?(?<body>.*)\z", RegexOptions.Singleline)]
    private static partial Regex FrontmatterPattern();
}
