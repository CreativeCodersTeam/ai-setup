using CreativeCoders.Core;

namespace AiSetup.Platform;

/// <summary>
/// Default <see cref="IFileSystem"/> implementation backed by <see cref="System.IO"/>.
/// </summary>
public sealed class FileSystem : IFileSystem
{
    /// <inheritdoc />
    public bool FileExists(string path)
    {
        Ensure.IsNotNullOrWhitespace(path);
        return File.Exists(path);
    }

    /// <inheritdoc />
    public bool DirectoryExists(string path)
    {
        Ensure.IsNotNullOrWhitespace(path);
        return Directory.Exists(path);
    }

    /// <inheritdoc />
    public string ReadAllText(string path)
    {
        Ensure.IsNotNullOrWhitespace(path);
        return File.ReadAllText(path);
    }

    /// <inheritdoc />
    public void WriteAllText(string path, string content)
    {
        Ensure.IsNotNullOrWhitespace(path);
        Ensure.NotNull(content);

        var directory = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.WriteAllText(path, content);
    }

    /// <inheritdoc />
    public void CopyFile(string sourcePath, string targetPath)
    {
        Ensure.IsNotNullOrWhitespace(sourcePath);
        Ensure.IsNotNullOrWhitespace(targetPath);

        var directory = Path.GetDirectoryName(targetPath);

        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        File.Copy(sourcePath, targetPath, overwrite: true);
    }

    /// <inheritdoc />
    public void CopyDirectory(string sourcePath, string targetPath)
    {
        Ensure.IsNotNullOrWhitespace(sourcePath);
        Ensure.IsNotNullOrWhitespace(targetPath);

        Directory.CreateDirectory(targetPath);

        foreach (var file in Directory.EnumerateFiles(sourcePath, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourcePath, file);
            var destination = Path.Combine(targetPath, relative);
            var destinationDir = Path.GetDirectoryName(destination);

            if (!string.IsNullOrEmpty(destinationDir))
            {
                Directory.CreateDirectory(destinationDir);
            }

            File.Copy(file, destination, overwrite: true);
        }
    }

    /// <inheritdoc />
    public void CreateDirectory(string path)
    {
        Ensure.IsNotNullOrWhitespace(path);
        Directory.CreateDirectory(path);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> EnumerateFilesRecursive(string path)
    {
        Ensure.IsNotNullOrWhitespace(path);

        if (!Directory.Exists(path))
        {
            return [];
        }

        return Directory
            .EnumerateFiles(path, "*", SearchOption.AllDirectories)
            .Select(f => Path.GetRelativePath(path, f))
            .ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> EnumerateDirectories(string path)
    {
        Ensure.IsNotNullOrWhitespace(path);

        if (!Directory.Exists(path))
        {
            return [];
        }

        return Directory.EnumerateDirectories(path).ToArray();
    }

    /// <inheritdoc />
    public void DeleteFile(string path)
    {
        Ensure.IsNotNullOrWhitespace(path);

        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}
