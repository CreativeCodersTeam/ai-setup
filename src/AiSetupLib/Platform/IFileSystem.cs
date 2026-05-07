namespace AiSetup.Platform;

/// <summary>
/// Thin abstraction over file-system primitives so the deploy logic can be tested without a real disk.
/// All paths are absolute.
/// </summary>
public interface IFileSystem
{
    /// <summary>True if the file at <paramref name="path"/> exists.</summary>
    bool FileExists(string path);

    /// <summary>True if the directory at <paramref name="path"/> exists.</summary>
    bool DirectoryExists(string path);

    /// <summary>Reads the file at <paramref name="path"/> as UTF-8 text.</summary>
    string ReadAllText(string path);

    /// <summary>Writes <paramref name="content"/> to <paramref name="path"/>, creating directories as needed.</summary>
    void WriteAllText(string path, string content);

    /// <summary>Copies <paramref name="sourcePath"/> to <paramref name="targetPath"/>.</summary>
    /// <param name="sourcePath">Existing source file.</param>
    /// <param name="targetPath">Destination path. Overwritten if it exists.</param>
    void CopyFile(string sourcePath, string targetPath);

    /// <summary>Recursively copies a directory.</summary>
    /// <param name="sourcePath">Existing source directory.</param>
    /// <param name="targetPath">Destination directory. Created (or merged) if it does not exist.</param>
    void CopyDirectory(string sourcePath, string targetPath);

    /// <summary>Creates the directory at <paramref name="path"/> (no-op if it already exists).</summary>
    void CreateDirectory(string path);

    /// <summary>Returns the relative paths of all files under <paramref name="path"/> (recursive).</summary>
    IReadOnlyList<string> EnumerateFilesRecursive(string path);

    /// <summary>Returns the immediate subdirectories of <paramref name="path"/>.</summary>
    IReadOnlyList<string> EnumerateDirectories(string path);

    /// <summary>Deletes <paramref name="path"/> if it exists.</summary>
    void DeleteFile(string path);
}
