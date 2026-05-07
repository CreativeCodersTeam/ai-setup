using System.Text.Json;
using System.Text.Json.Nodes;
using AiSetup.Exceptions;
using AiSetup.Models;
using CreativeCoders.Core;
using YamlDotNet.Serialization;

namespace AiSetup.Aggregation;

/// <summary>
/// Default <see cref="IMcpConfigMerger"/> using YamlDotNet for parsing and
/// <see cref="JsonNode"/> for merging.
/// </summary>
public sealed class McpConfigMerger : IMcpConfigMerger
{
    private static readonly IDeserializer Deserializer = new DeserializerBuilder().Build();

    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    /// <inheritdoc />
    public string Merge(
        IReadOnlyList<AssetDefinition> configs,
        string serversKey,
        string? existingJson,
        bool overwriteOnConflict)
    {
        Ensure.NotNull(configs);
        Ensure.IsNotNullOrWhitespace(serversKey);

        var root = ParseRoot(existingJson);

        if (root[serversKey] is not JsonObject servers)
        {
            servers = [];
            root[serversKey] = servers;
        }

        foreach (var config in configs)
        {
            if (config.Type != AssetType.McpConfig)
            {
                throw new AiSetupException($"Asset '{config.Id}' is not an MCP config.");
            }

            var (name, entry) = ConvertToServerEntry(config);

            if (servers.ContainsKey(name) && !overwriteOnConflict)
            {
                throw new AiSetupException(
                    $"MCP server '{name}' already exists in the target settings. Use --force to overwrite.");
            }

            servers[name] = entry;
        }

        return root.ToJsonString(WriteOptions);
    }

    private static JsonObject ParseRoot(string? existingJson)
    {
        if (string.IsNullOrWhiteSpace(existingJson))
        {
            return [];
        }

        try
        {
            var parsed = JsonNode.Parse(existingJson) as JsonObject;
            return parsed ?? [];
        }
        catch (JsonException ex)
        {
            throw new AiSetupException($"Existing MCP settings file is not valid JSON: {ex.Message}", ex);
        }
    }

    private static (string Name, JsonNode Entry) ConvertToServerEntry(AssetDefinition config)
    {
        var raw = Deserializer.Deserialize<Dictionary<object, object?>>(config.Body)
                  ?? throw new AiSetupException($"MCP config '{config.Id}' is empty.");

        var name = raw.TryGetValue("name", out var nameValue) && nameValue is not null
            ? nameValue.ToString()!
            : config.Id;

        var entry = new JsonObject();

        foreach (var (rawKey, rawValue) in raw)
        {
            var key = rawKey.ToString();

            if (string.IsNullOrEmpty(key) || string.Equals(key, "name", StringComparison.Ordinal))
            {
                continue;
            }

            entry[key] = ToJsonNode(rawValue);
        }

        return (name, entry);
    }

    private static JsonNode? ToJsonNode(object? value)
    {
        return value switch
        {
            null => null,
            string s => JsonValue.Create(s),
            bool b => JsonValue.Create(b),
            int i => JsonValue.Create(i),
            long l => JsonValue.Create(l),
            double d => JsonValue.Create(d),
            IDictionary<object, object?> dict => DictToJson(dict),
            IEnumerable<object?> list => ListToJson(list),
            _ => JsonValue.Create(value.ToString())
        };
    }

    private static JsonNode DictToJson(IDictionary<object, object?> dict)
    {
        var obj = new JsonObject();

        foreach (var (k, v) in dict)
        {
            var key = k.ToString();

            if (!string.IsNullOrEmpty(key))
            {
                obj[key] = ToJsonNode(v);
            }
        }

        return obj;
    }

    private static JsonNode ListToJson(IEnumerable<object?> list)
    {
        var array = new JsonArray();

        foreach (var item in list)
        {
            array.Add(ToJsonNode(item));
        }

        return array;
    }
}
