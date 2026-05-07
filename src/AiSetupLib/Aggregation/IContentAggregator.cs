using AiSetup.Lib.Models;

namespace AiSetup.Lib.Aggregation;

/// <summary>
/// Aggregates multiple <see cref="AssetDefinition"/>s into a single output
/// document (e.g. <c>CLAUDE.md</c>).
/// </summary>
public interface IContentAggregator
{
    /// <summary>
    /// Concatenates the bodies of <paramref name="assets"/> with stable
    /// ordering and per-asset headers.
    /// </summary>
    string Aggregate(IEnumerable<AssetDefinition> assets);
}
