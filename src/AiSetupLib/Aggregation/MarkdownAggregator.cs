using System.Text;
using AiSetup.Lib.Models;
using CreativeCoders.Core;

namespace AiSetup.Lib.Aggregation;

/// <inheritdoc cref="IContentAggregator"/>
public sealed class MarkdownAggregator : IContentAggregator
{
    /// <inheritdoc />
    public string Aggregate(IEnumerable<AssetDefinition> assets)
    {
        Ensure.NotNull(assets, nameof(assets));

        var ordered = assets
            .OrderBy(a => OrderRank(a.Type))
            .ThenBy(a => a.Name, StringComparer.Ordinal)
            .ToArray();

        var sb = new StringBuilder();

        for (var i = 0; i < ordered.Length; i++)
        {
            var asset = ordered[i];

            if (i > 0)
            {
                sb.AppendLine();
                sb.AppendLine("---");
                sb.AppendLine();
            }

            sb.Append("# ").AppendLine(asset.Name);

            if (!string.IsNullOrWhiteSpace(asset.Description))
            {
                sb.AppendLine();
                sb.AppendLine(asset.Description);
            }

            if (!string.IsNullOrWhiteSpace(asset.Body))
            {
                sb.AppendLine();
                sb.Append(asset.Body.TrimEnd());
                sb.AppendLine();
            }
        }

        return sb.ToString();
    }

    private static int OrderRank(AssetType type) => type switch
    {
        AssetType.Instruction => 0,
        AssetType.Agent => 1,
        AssetType.Skill => 2,
        AssetType.McpConfig => 3,
        _ => 99,
    };
}
