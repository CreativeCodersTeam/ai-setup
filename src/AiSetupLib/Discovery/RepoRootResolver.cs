using AiSetup.Lib.IO;
using CreativeCoders.Core;

namespace AiSetup.Lib.Discovery;

/// <summary>
/// Locates the source repository root by walking up from a starting directory
/// until a marker (<c>.git</c> directory or any of the well-known asset
/// folders) is found.
/// </summary>
public sealed class RepoRootResolver
{
    private static readonly string[] _markerDirectories = ["agents", "instructions", "skills", "profiles"];

    private readonly IFileSystem _fileSystem;

    /// <summary>Initialises a new instance.</summary>
    public RepoRootResolver(IFileSystem fileSystem)
    {
        _fileSystem = Ensure.NotNull(fileSystem, nameof(fileSystem));
    }

    /// <summary>
    /// Walks upward from <paramref name="startPath"/> until a repository marker
    /// is detected. Returns null when no marker is found.
    /// </summary>
    public string? Resolve(string startPath)
    {
        Ensure.IsNotNullOrWhitespace(startPath, nameof(startPath));

        var current = Path.GetFullPath(startPath);

        while (!string.IsNullOrEmpty(current))
        {
            if (_fileSystem.DirectoryExists(Path.Combine(current, ".git")))
            {
                return current;
            }

            foreach (var marker in _markerDirectories)
            {
                if (_fileSystem.DirectoryExists(Path.Combine(current, marker)))
                {
                    return current;
                }
            }

            var parent = Path.GetDirectoryName(current);

            if (string.IsNullOrEmpty(parent) || string.Equals(parent, current, StringComparison.Ordinal))
            {
                return null;
            }

            current = parent;
        }

        return null;
    }
}
