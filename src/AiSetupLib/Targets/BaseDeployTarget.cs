using AiSetup.Lib.IO;
using AiSetup.Lib.Models;
using AiSetup.Lib.Targets.Platform;
using CreativeCoders.Core;

namespace AiSetup.Lib.Targets;

/// <summary>
/// Common base class for <see cref="IDeployTarget"/> implementations.
/// </summary>
public abstract class BaseDeployTarget : IDeployTarget
{
    /// <summary>File system used for read access during planning.</summary>
    protected IFileSystem FileSystem { get; }

    /// <summary>Provides platform-specific local install paths.</summary>
    protected IPlatformPathProvider PathProvider { get; }

    /// <summary>Initialises the base target.</summary>
    protected BaseDeployTarget(IFileSystem fileSystem, IPlatformPathProvider pathProvider)
    {
        FileSystem = Ensure.NotNull(fileSystem, nameof(fileSystem));
        PathProvider = Ensure.NotNull(pathProvider, nameof(pathProvider));
    }

    /// <inheritdoc />
    public abstract DeployTarget Target { get; }

    /// <inheritdoc />
    public DeployPlan Plan(IReadOnlyList<AssetDefinition> assets, DeployOptions options)
    {
        Ensure.NotNull(assets, nameof(assets));
        Ensure.NotNull(options, nameof(options));

        var rootDirectory = ResolveRootDirectory(options);
        var actions = BuildActions(assets, options, rootDirectory).ToArray();

        return new DeployPlan(Target, options.Mode, actions);
    }

    /// <summary>Returns the root directory writes are anchored to.</summary>
    protected string ResolveRootDirectory(DeployOptions options)
    {
        if (options.Mode == DeployMode.Repo)
        {
            if (string.IsNullOrWhiteSpace(options.RepoPath))
            {
                throw new ArgumentException("RepoPath is required when Mode is Repo.", nameof(options));
            }

            return options.RepoPath;
        }

        return PathProvider.GetLocalRoot(Target);
    }

    /// <summary>
    /// Builds the planned actions for the target. Implementations decide
    /// whether to use <see cref="DeployActionKind.Create"/> or
    /// <see cref="DeployActionKind.Overwrite"/> based on existing target state.
    /// </summary>
    protected abstract IEnumerable<DeployAction> BuildActions(
        IReadOnlyList<AssetDefinition> assets,
        DeployOptions options,
        string rootDirectory);

    /// <summary>
    /// Convenience helper that classifies a write action by checking whether
    /// the target file/folder already exists.
    /// </summary>
    protected DeployActionKind ClassifyKind(string targetPath, bool isDirectory)
    {
        var exists = isDirectory ? FileSystem.DirectoryExists(targetPath) : FileSystem.FileExists(targetPath);

        return exists ? DeployActionKind.Overwrite : DeployActionKind.Create;
    }
}
