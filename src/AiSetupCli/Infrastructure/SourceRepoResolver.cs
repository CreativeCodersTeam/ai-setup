using AiSetup.Exceptions;
using AiSetup.Platform;
using CreativeCoders.Core;

namespace AiSetup.Cli.Infrastructure;

/// <summary>
/// Resolves the source ai-setup repository path from the optional CLI override,
/// falling back to <see cref="Environment.CurrentDirectory"/>. The result is
/// always an absolute path that is guaranteed to exist.
/// </summary>
internal static class SourceRepoResolver
{
    /// <summary>Resolves the source repository path.</summary>
    /// <param name="fileSystem">File system used to check the candidate directory.</param>
    /// <param name="candidate">Optional override (e.g. from <c>--source</c>).</param>
    /// <returns>Absolute path of the source repository.</returns>
    /// <exception cref="AiSetupException">Thrown when the resolved path does not exist.</exception>
    public static string Resolve(IFileSystem fileSystem, string? candidate)
    {
        Ensure.NotNull(fileSystem);

        var path = string.IsNullOrWhiteSpace(candidate)
            ? Environment.CurrentDirectory
            : candidate;

        if (!fileSystem.DirectoryExists(path))
        {
            throw new AiSetupException($"Source repository path '{path}' does not exist.");
        }

        return Path.GetFullPath(path);
    }
}
