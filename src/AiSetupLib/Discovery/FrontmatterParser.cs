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
        if (!source.StartsWith("---", StringComparison.Ordinal))
            return new FrontmatterResult(false, EmptyFrontmatter, source);

        var lines = source.Split('\n');
        var closingIndex = -1;
        for (var i = 1; i < lines.Length; i++)
        {
            if (lines[i].TrimEnd('\r') == "---")
            {
                closingIndex = i;
                break;
            }
        }

        if (closingIndex < 0)
            return new FrontmatterResult(false, EmptyFrontmatter, source);

        var yaml = string.Join('\n', lines[1..closingIndex]);
        var body = string.Join('\n', lines[(closingIndex + 1)..]).TrimStart('\n');

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
