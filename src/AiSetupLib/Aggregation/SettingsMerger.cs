using System.Text.Json;
using System.Text.Json.Nodes;
using AiSetup.Exceptions;
using AiSetup.Models;
using CreativeCoders.Core;

namespace AiSetup.Aggregation;

/// <summary>
/// Default <see cref="ISettingsMerger"/> using <see cref="JsonNode"/> for parsing and merging.
/// </summary>
public sealed class SettingsMerger : ISettingsMerger
{
    private static readonly JsonSerializerOptions WriteOptions = new()
    {
        WriteIndented = true
    };

    /// <inheritdoc />
    public string Merge(
        IReadOnlyList<AssetDefinition> settings,
        string? existingJson,
        McpConflictResolution conflictResolution)
    {
        Ensure.NotNull(settings);

        var root = ParseBaseObject(existingJson);

        foreach (var asset in settings)
        {
            if (asset.Type != AssetType.Settings)
            {
                throw new AiSetupException($"Asset '{asset.Id}' is not a settings fragment.");
            }

            var fragment = ParseFragmentObject(asset);

            MergeInto(root, fragment, conflictResolution, asset.Id, path: string.Empty);
        }

        return root.ToJsonString(WriteOptions);
    }

    private static JsonObject ParseBaseObject(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        var parsed = Parse(json, "Existing settings file");

        return parsed as JsonObject
            ?? throw new AiSetupException("Existing settings file must be a JSON object.");
    }

    private static JsonObject ParseFragmentObject(AssetDefinition asset)
    {
        var parsed = Parse(asset.Body, $"Settings fragment '{asset.Id}'")
            ?? throw new AiSetupException($"Settings fragment '{asset.Id}' is empty.");

        return parsed as JsonObject
            ?? throw new AiSetupException($"Settings fragment '{asset.Id}' must be a JSON object.");
    }

    private static JsonNode? Parse(string json, string what)
    {
        try
        {
            return JsonNode.Parse(json);
        }
        catch (JsonException ex)
        {
            throw new AiSetupException($"{what} is not valid JSON: {ex.Message}", ex);
        }
    }

    private static void MergeInto(
        JsonObject target,
        JsonObject source,
        McpConflictResolution conflict,
        string sourceId,
        string path)
    {
        foreach (var (key, incoming) in source.ToArray())
        {
            var keyPath = string.IsNullOrEmpty(path) ? key : $"{path}.{key}";

            if (!target.TryGetPropertyValue(key, out var current) || current is null)
            {
                target[key] = incoming?.DeepClone();

                continue;
            }

            if (current is JsonObject currentObj && incoming is JsonObject incomingObj)
            {
                MergeInto(currentObj, incomingObj, conflict, sourceId, keyPath);

                continue;
            }

            if (current is JsonArray currentArr && incoming is JsonArray incomingArr)
            {
                UnionInto(currentArr, incomingArr);

                continue;
            }

            // Scalar or type mismatch.
            if (JsonNode.DeepEquals(current, incoming))
            {
                continue;
            }

            switch (conflict)
            {
                case McpConflictResolution.Fail:
                    throw new AiSetupException(
                        $"Settings key '{keyPath}' from '{sourceId}' conflicts with an existing value. " +
                        "Use '--mcp-on-conflict overwrite' or '--mcp-on-conflict skip'.");
                case McpConflictResolution.Skip:
                    continue;
                case McpConflictResolution.Overwrite:
                    target[key] = incoming?.DeepClone();

                    break;
                default:
                    throw new InvalidOperationException($"Unhandled conflict resolution '{conflict}'.");
            }
        }
    }

    private static void UnionInto(JsonArray target, JsonArray incoming)
    {
        var seen = new HashSet<string>(target.Select(ToComparisonKey));

        foreach (var item in incoming)
        {
            if (seen.Add(ToComparisonKey(item)))
            {
                target.Add(item?.DeepClone());
            }
        }
    }

    private static string ToComparisonKey(JsonNode? node)
    {
        return node?.ToJsonString() ?? "null";
    }
}
