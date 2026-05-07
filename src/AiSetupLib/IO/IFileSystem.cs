namespace AiSetup.Lib.IO;

/// <summary>
/// Thin abstraction over <see cref="System.IO"/> file operations to enable
/// in-memory testing.
/// </summary>
public interface IFileSystem
{
    /// <summary>Returns true if the given file exists.</summary>
    bool FileExists(string path);

    /// <summary>Returns true if the given directory exists.</summary>
    bool DirectoryExists(string path);

    /// <summary>Reads all text from a file.</summary>
    string ReadAllText(string path);

    /// <summary>Writes text to a file, creating parent directories as needed.</summary>
    void WriteAllText(string path, string content);

    /// <summary>Creates the directory and all parents.</summary>
    void CreateDirectory(string path);

    /// <summary>Returns the immediate child directories of <paramref name="path"/>.</summary>
    IReadOnlyList<string> EnumerateDirectories(string path);

    /// <summary>Returns files in <paramref name="path"/> that match the given search pattern.</summary>
    IReadOnlyList<string> EnumerateFiles(string path, string searchPattern, bool recursive);

    /// <summary>Recursively copies a directory's content.</summary>
    void CopyDirectory(string sourceDirectory, string targetDirectory, bool overwrite);
}
