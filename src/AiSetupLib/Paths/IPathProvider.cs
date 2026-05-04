using AiSetupLib.Models;

namespace AiSetupLib.Paths;

public interface IPathProvider
{
    string GetLocalRoot(DeployTarget target);
    string GetRepoSubPath(DeployTarget target, AssetType type);
    string GetClaudeAggregatedFileName();
    string GetMcpSettingsRelativePath(DeployTarget target);
}
