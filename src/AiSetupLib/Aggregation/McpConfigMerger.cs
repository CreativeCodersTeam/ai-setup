using AiSetupLib.Models;
using YamlDotNet.Core;
using YamlDotNet.RepresentationModel;

namespace AiSetupLib.Aggregation;

public sealed class McpConfigMerger
{
    public IReadOnlyDictionary<string, object?> Merge(IReadOnlyList<AssetDefinition> mcpAssets)
    {
        var result = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var asset in mcpAssets)
        {
            if (string.IsNullOrWhiteSpace(asset.Body))
                continue;

            var stream = new YamlStream();
            try
            {
                stream.Load(new StringReader(asset.Body));
            }
            catch (YamlException)
            {
                continue;
            }
            if (stream.Documents.Count == 0
                || stream.Documents[0].RootNode is not YamlMappingNode root)
            {
                continue;
            }
            result[asset.Name] = ConvertMapping(root);
        }
        return result;
    }

    private static object? ConvertNode(YamlNode node) => node switch
    {
        YamlScalarNode s => s.Value,
        YamlSequenceNode seq => seq.Children.Select(ConvertNode).ToList(),
        YamlMappingNode m => ConvertMapping(m),
        _ => null,
    };

    private static Dictionary<string, object?> ConvertMapping(YamlMappingNode node)
    {
        var dict = new Dictionary<string, object?>(StringComparer.Ordinal);
        foreach (var (key, value) in node.Children)
        {
            if (key is YamlScalarNode k && k.Value is { } name)
                dict[name] = ConvertNode(value);
        }
        return dict;
    }
}
