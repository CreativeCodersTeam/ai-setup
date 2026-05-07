using System.Text.Json;
using System.Text.Json.Nodes;
using AiSetup.Lib.Models;
using CreativeCoders.Core;
using YamlDotNet.Serialization;

namespace AiSetup.Lib.Aggregation;

/// <inheritdoc cref="IMcpConfigMerger"/>
public sealed class McpConfigMerger : IMcpConfigMerger
{
    private static readonly JsonSerializerOptions _writeOptions = new()
    {
        WriteIndented = true,
    };

    private readonly IDeserializer _yamlDeserializer = new DeserializerBuilder().Build();

    /// <inheritdoc />
    public string Merge(IEnumerable<AssetDefinition> assets, string? existingJson, string? settingsRootKey)
    {
        Ensure.NotNull(assets, nameof(assets));

        var root = LoadRoot(existingJson);
        var serversNode = GetOrCreateServersNode(root, settingsRootKey);

        foreach (var asset in assets)
        {
            var yaml = string.IsNullOrWhiteSpace(asset.Body) ? asset.RawContent : asset.Body;
            var entry = ConvertYamlToJsonNode(yaml);
            serversNode[asset.Name] = entry;
        }

        return root.ToJsonString(_writeOptions) + Environment.NewLine;
    }

    private static JsonObject LoadRoot(string? existingJson)
    {
        if (string.IsNullOrWhiteSpace(existingJson))
        {
            return new JsonObject();
        }

        var node = JsonNode.Parse(existingJson);

        if (node is JsonObject obj)
        {
            return obj;
        }

        return new JsonObject();
    }

    private static JsonObject GetOrCreateServersNode(JsonObject root, string? settingsRootKey)
    {
        if (string.IsNullOrEmpty(settingsRootKey))
        {
            return root;
        }

        if (root[settingsRootKey] is JsonObject existing)
        {
            return existing;
        }

        var fresh = new JsonObject();
        root[settingsRootKey] = fresh;

        return fresh;
    }

    private JsonNode ConvertYamlToJsonNode(string yaml)
    {
        var obj = _yamlDeserializer.Deserialize<object?>(yaml);
        var json = JsonSerializer.Serialize(obj);

        return JsonNode.Parse(json) ?? new JsonObject();
    }
}
