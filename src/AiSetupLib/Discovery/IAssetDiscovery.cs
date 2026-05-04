using AiSetupLib.Models;

namespace AiSetupLib.Discovery;

public interface IAssetDiscovery
{
    IReadOnlyList<AssetDefinition> Discover(string repoRoot);
}
