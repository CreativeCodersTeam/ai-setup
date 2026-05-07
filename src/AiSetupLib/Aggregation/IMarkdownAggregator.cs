using AiSetup.Models;

namespace AiSetup.Aggregation;

/// <summary>
/// Aggregates multiple Markdown assets into a single document for targets that
/// expect a combined file (e.g. Claude Code's CLAUDE.md).
/// </summary>
public interface IMarkdownAggregator
{
    /// <summary>
    /// Builds a combined Markdown document from the given assets in a stable order.
    /// </summary>
    /// <param name="assets">Assets to aggregate. Order is normalized internally.</param>
    /// <returns>Combined Markdown text ready to write to disk.</returns>
    string Aggregate(IReadOnlyList<AssetDefinition> assets);
}
