using AiSetupLib.Models;

namespace AiSetupLib.Aggregation;

public interface IContentAggregator
{
    string Aggregate(IReadOnlyList<AssetDefinition> assets);
}
