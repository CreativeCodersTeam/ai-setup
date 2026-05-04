using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace AiSetupLib.Discovery;

public sealed record FrontmatterResult(
    bool HasFrontmatter,
    IReadOnlyDictionary<string, object?> Frontmatter,
    string Body);

public sealed class FrontmatterParser
{
    private static readonly IReadOnlyDictionary<string, object?> EmptyFrontmatter
        = new Dictionary<string, object?>();

    public FrontmatterResult Parse(string source)
    {
        if (!TryFindFences(source, out var yamlStart, out var yamlEnd, out var bodyStart))
            return new FrontmatterResult(false, EmptyFrontmatter, source);

        var yaml = source.Substring(yamlStart, yamlEnd - yamlStart);
        var body = bodyStart >= source.Length ? string.Empty : source[bodyStart..];

        try
        {
            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            if (stream.Documents.Count == 0
                || stream.Documents[0].RootNode is not YamlMappingNode root)
            {
                return new FrontmatterResult(false, EmptyFrontmatter, source);
            }
            var dict = ConvertMapping(root);
            return new FrontmatterResult(true, dict, body);
        }
        catch (YamlException)
        {
            return new FrontmatterResult(false, EmptyFrontmatter, source);
        }
    }

    /// <summary>
    /// Locate the opening and closing <c>---</c> fences without altering bytes.
    /// On success, <paramref name="yamlStart"/> is the index of the first YAML byte
    /// (just past the opening fence's terminating newline), <paramref name="yamlEnd"/>
    /// is the index just past the last YAML byte (i.e. the index of the newline that
    /// precedes the closing fence), and <paramref name="bodyStart"/> is the index of
    /// the first body byte (just past the closing fence's terminating newline, or
    /// equal to <c>source.Length</c> if there is no body).
    /// </summary>
    private static bool TryFindFences(string source, out int yamlStart, out int yamlEnd, out int bodyStart)
    {
        yamlStart = 0;
        yamlEnd = 0;
        bodyStart = 0;

        // Opening fence must be "---" at position 0, followed by \n, \r\n, or end-of-string.
        if (!source.StartsWith("---", StringComparison.Ordinal))
            return false;

        int afterOpen;
        if (source.Length == 3)
        {
            // Bare "---" with nothing after — no closing fence possible.
            return false;
        }
        else if (source[3] == '\n')
        {
            afterOpen = 4;
        }
        else if (source[3] == '\r' && source.Length >= 5 && source[4] == '\n')
        {
            afterOpen = 5;
        }
        else
        {
            return false;
        }

        yamlStart = afterOpen;

        // Scan for closing fence: a line whose content is exactly "---".
        // Look for "\n---" followed by \n, \r\n, or end-of-string.
        // Closing fence must be on its own line, so the byte preceding "---"
        // must be '\n' (which also covers the CRLF case since '\r' precedes '\n').
        var i = afterOpen;
        while (i < source.Length)
        {
            // Need at least 4 chars: '\n', '-', '-', '-'
            if (source[i] == '\n' && i + 3 < source.Length
                && source[i + 1] == '-' && source[i + 2] == '-' && source[i + 3] == '-')
            {
                var fenceLineStart = i + 1; // index of first '-'
                var afterDashes = i + 4;    // index just past "---"

                // YAML region ends just before the newline at i. Exclude a
                // preceding '\r' (CRLF) so it isn't fed to the YAML parser.
                var ye = (i > yamlStart && source[i - 1] == '\r') ? i - 1 : i;

                // Closing fence ends with \n, \r\n, or end-of-string.
                if (afterDashes == source.Length)
                {
                    yamlEnd = ye;
                    bodyStart = source.Length;
                    return true;
                }
                if (source[afterDashes] == '\n')
                {
                    yamlEnd = ye;
                    bodyStart = afterDashes + 1;
                    return true;
                }
                if (source[afterDashes] == '\r'
                    && afterDashes + 1 < source.Length
                    && source[afterDashes + 1] == '\n')
                {
                    yamlEnd = ye;
                    bodyStart = afterDashes + 2;
                    return true;
                }
                // "---" not the entire line — keep scanning.
                i = fenceLineStart;
            }
            i++;
        }

        // Special case: closing fence at very start of YAML region (yamlStart points at '-').
        // E.g. "---\n---" — already handled above only if there's a preceding '\n'.
        // Check directly:
        if (source.Length - yamlStart >= 3
            && source[yamlStart] == '-' && source[yamlStart + 1] == '-' && source[yamlStart + 2] == '-')
        {
            var afterDashes = yamlStart + 3;
            if (afterDashes == source.Length)
            {
                yamlEnd = yamlStart;
                bodyStart = source.Length;
                return true;
            }
            if (source[afterDashes] == '\n')
            {
                yamlEnd = yamlStart;
                bodyStart = afterDashes + 1;
                return true;
            }
            if (source[afterDashes] == '\r'
                && afterDashes + 1 < source.Length
                && source[afterDashes + 1] == '\n')
            {
                yamlEnd = yamlStart;
                bodyStart = afterDashes + 2;
                return true;
            }
        }

        return false;
    }

    private static Dictionary<string, object?> ConvertMapping(YamlMappingNode node)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in node.Children)
        {
            if (key is YamlScalarNode scalarKey && scalarKey.Value is { } k)
                result[k] = ConvertNode(value);
        }
        return result;
    }

    private static object? ConvertNode(YamlNode node) => node switch
    {
        YamlScalarNode s => s.Value,
        YamlSequenceNode seq => (IReadOnlyList<string>)seq.Children
            .OfType<YamlScalarNode>()
            .Select(c => c.Value ?? "")
            .ToList()
            .AsReadOnly(),
        YamlMappingNode m => ConvertMapping(m),
        _ => null,
    };
}
