using CreativeCoders.Core;

namespace AiSetup.Lib.IO;

/// <inheritdoc cref="IFileSystem"/>
public sealed class FileSystem : IFileSystem
{
    /// <inheritdoc />
    public bool FileExists(string path)
    {
        Ensure.IsNotNullOrWhitespace(path, nameof(path));

        return File.Exists(path);
    }

    /// <inheritdoc />
    public bool DirectoryExists(string path)
    {
        Ensure.IsNotNullOrWhitespace(path, nameof(path));

        return Directory.Exists(path);
    }

    /// <inheritdoc />
    public string ReadAllText(string path)
    {
        Ensure.IsNotNullOrWhitespace(path, nameof(path));

        return File.ReadAllText(path);
    }

    /// <inheritdoc />
    public void WriteAllText(string path, string content)
    {
        Ensure.IsNotNullOrWhitespace(path, nameof(path));
        Ensure.NotNull(content, nameof(content));

        var dir = Path.GetDirectoryName(path);

        if (!string.IsNullOrEmpty(dir))
        {
            Directory.CreateDirectory(dir);
        }

        File.WriteAllText(path, content);
    }

    /// <inheritdoc />
    public void CreateDirectory(string path)
    {
        Ensure.IsNotNullOrWhitespace(path, nameof(path));

        Directory.CreateDirectory(path);
    }

    /// <inheritdoc />
    public IReadOnlyList<string> EnumerateDirectories(string path)
    {
        Ensure.IsNotNullOrWhitespace(path, nameof(path));

        if (!Directory.Exists(path))
        {
            return Array.Empty<string>();
        }

        return Directory.EnumerateDirectories(path).ToArray();
    }

    /// <inheritdoc />
    public IReadOnlyList<string> EnumerateFiles(string path, string searchPattern, bool recursive)
    {
        Ensure.IsNotNullOrWhitespace(path, nameof(path));
        Ensure.IsNotNullOrWhitespace(searchPattern, nameof(searchPattern));

        if (!Directory.Exists(path))
        {
            return Array.Empty<string>();
        }

        var option = recursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly;

        return Directory.EnumerateFiles(path, searchPattern, option).ToArray();
    }

    /// <inheritdoc />
    public void CopyDirectory(string sourceDirectory, string targetDirectory, bool overwrite)
    {
        Ensure.IsNotNullOrWhitespace(sourceDirectory, nameof(sourceDirectory));
        Ensure.IsNotNullOrWhitespace(targetDirectory, nameof(targetDirectory));

        Directory.CreateDirectory(targetDirectory);

        foreach (var file in Directory.EnumerateFiles(sourceDirectory, "*", SearchOption.AllDirectories))
        {
            var relative = Path.GetRelativePath(sourceDirectory, file);
            var dest = Path.Combine(targetDirectory, relative);
            var destDir = Path.GetDirectoryName(dest);

            if (!string.IsNullOrEmpty(destDir))
            {
                Directory.CreateDirectory(destDir);
            }

            File.Copy(file, dest, overwrite);
        }
    }
}
