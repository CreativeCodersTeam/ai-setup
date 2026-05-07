using System.Text;
using AiSetup.Models;
using CreativeCoders.Core;

namespace AiSetup.Aggregation;

/// <summary>
/// Default <see cref="IMarkdownAggregator"/>. Emits sections in the order
/// instructions → agents and prepends a generated banner. Within the
/// instructions group, assets whose ID starts with <c>general/</c> are emitted
/// first; the original input order is preserved within and across groups.
/// </summary>
public sealed class MarkdownAggregator : IMarkdownAggregator
{
    /// <inheritdoc />
    public string Aggregate(IReadOnlyList<AssetDefinition> assets)
    {
        Ensure.NotNull(assets);

        var ordered = assets
            .Where(a => a.Type is AssetType.Instruction or AssetType.Agent)
            .Select((a, i) => (Asset: a, Index: i))
            .OrderBy(x => x.Asset.Type == AssetType.Instruction ? 0 : 1)
            .ThenBy(x => x.Asset.Type == AssetType.Instruction && IsGeneral(x.Asset) ? 0 : 1)
            .ThenBy(x => x.Index)
            .Select(x => x.Asset)
            .ToArray();

        var builder = new StringBuilder();
        foreach (var asset in ordered)
        {
            AppendSection(builder, asset);
        }

        return builder.ToString();
    }

    private static void AppendSection(StringBuilder builder, AssetDefinition asset)
    {
        builder.Append("## ").AppendLine(asset.Name);

        if (!string.IsNullOrWhiteSpace(asset.Description))
        {
            builder.AppendLine();
            builder.AppendLine(asset.Description.Trim());
        }

        if (!string.IsNullOrWhiteSpace(asset.ApplyTo))
        {
            builder.AppendLine();
            builder.Append("> Applies to: `").Append(asset.ApplyTo).AppendLine("`");
        }

        builder.AppendLine();
        builder.AppendLine(asset.Body.TrimEnd());
        builder.AppendLine();
    }

    private static bool IsGeneral(AssetDefinition asset)
        => asset.Id.StartsWith("general/", StringComparison.Ordinal);
}
