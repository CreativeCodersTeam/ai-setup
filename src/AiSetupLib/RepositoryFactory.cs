using AiSetup.Discovery;
using AiSetup.Platform;
using AiSetup.Profiles;
using CreativeCoders.Core;

namespace AiSetup;

/// <summary>
/// Default <see cref="IRepositoryFactory"/> backed by the registered <see cref="IFileSystem"/>.
/// </summary>
public sealed class RepositoryFactory : IRepositoryFactory
{
    private readonly IFileSystem _fileSystem;

    /// <summary>Initializes a new instance.</summary>
    /// <param name="fileSystem">File system abstraction used by the produced repositories.</param>
    public RepositoryFactory(IFileSystem fileSystem)
    {
        _fileSystem = Ensure.NotNull(fileSystem);
    }

    /// <inheritdoc />
    public IAssetRepository CreateAssets(string sourceRepoPath)
    {
        Ensure.IsNotNullOrWhitespace(sourceRepoPath);
        return new FileSystemAssetRepository(_fileSystem, sourceRepoPath);
    }

    /// <inheritdoc />
    public IProfileRepository CreateProfiles(string sourceRepoPath)
    {
        Ensure.IsNotNullOrWhitespace(sourceRepoPath);
        return new YamlProfileRepository(_fileSystem, sourceRepoPath);
    }
}
