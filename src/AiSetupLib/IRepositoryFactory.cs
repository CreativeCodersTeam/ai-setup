using AiSetup.Discovery;
using AiSetup.Profiles;

namespace AiSetup;

/// <summary>
/// Builds <see cref="IAssetRepository"/> and <see cref="IProfileRepository"/> instances bound
/// to a specific source repository root. Used by the CLI commands to keep the asset / profile
/// repositories per-invocation while leaving everything else in DI.
/// </summary>
public interface IRepositoryFactory
{
    /// <summary>Creates an asset repository for <paramref name="sourceRepoPath"/>.</summary>
    /// <param name="sourceRepoPath">Absolute path of the ai-setup source repository.</param>
    IAssetRepository CreateAssets(string sourceRepoPath);

    /// <summary>Creates a profile repository for <paramref name="sourceRepoPath"/>.</summary>
    /// <param name="sourceRepoPath">Absolute path of the ai-setup source repository.</param>
    IProfileRepository CreateProfiles(string sourceRepoPath);
}
