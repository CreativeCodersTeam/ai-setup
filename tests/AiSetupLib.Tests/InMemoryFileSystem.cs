using AiSetup.Lib.IO;

namespace AiSetup.Lib.Tests;

internal sealed class InMemoryFileSystem : IFileSystem
{
    private readonly Dictionary<string, string> _files = new(StringComparer.Ordinal);
    private readonly HashSet<string> _directories = new(StringComparer.Ordinal);

    public IReadOnlyDictionary<string, string> Files => _files;

    public InMemoryFileSystem AddFile(string path, string content)
    {
        var normalized = Normalize(path);
        _files[normalized] = content;
        AddParentDirectories(normalized);
        return this;
    }

    public InMemoryFileSystem AddDirectory(string path)
    {
        _directories.Add(Normalize(path));
        return this;
    }

    public bool FileExists(string path) => _files.ContainsKey(Normalize(path));

    public bool DirectoryExists(string path)
    {
        var normalized = Normalize(path);
        if (_directories.Contains(normalized))
        {
            return true;
        }

        var prefix = normalized + Path.DirectorySeparatorChar;
        return _files.Keys.Any(k => k.StartsWith(prefix, StringComparison.Ordinal))
            || _directories.Any(d => d.StartsWith(prefix, StringComparison.Ordinal));
    }

    public string ReadAllText(string path) => _files[Normalize(path)];

    public void WriteAllText(string path, string content)
    {
        var normalized = Normalize(path);
        _files[normalized] = content;
        AddParentDirectories(normalized);
    }

    public void CreateDirectory(string path) => _directories.Add(Normalize(path));

    public IReadOnlyList<string> EnumerateDirectories(string path)
    {
        var normalized = Normalize(path);
        var prefix = normalized + Path.DirectorySeparatorChar;
        var result = new HashSet<string>(StringComparer.Ordinal);

        foreach (var dir in _directories)
        {
            if (!dir.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            var rest = dir[prefix.Length..];
            var firstSep = rest.IndexOf(Path.DirectorySeparatorChar);
            var firstSegment = firstSep < 0 ? rest : rest[..firstSep];

            if (!string.IsNullOrEmpty(firstSegment))
            {
                result.Add(prefix + firstSegment);
            }
        }

        foreach (var file in _files.Keys)
        {
            if (!file.StartsWith(prefix, StringComparison.Ordinal))
            {
                continue;
            }

            var rest = file[prefix.Length..];
            var firstSep = rest.IndexOf(Path.DirectorySeparatorChar);

            if (firstSep > 0)
            {
                result.Add(prefix + rest[..firstSep]);
            }
        }

        return result.ToArray();
    }

    public IReadOnlyList<string> EnumerateFiles(string path, string searchPattern, bool recursive)
    {
        var normalized = Normalize(path);
        var prefix = normalized + Path.DirectorySeparatorChar;
        var matcher = BuildMatcher(searchPattern);

        return _files.Keys
            .Where(k => k.StartsWith(prefix, StringComparison.Ordinal))
            .Where(k =>
            {
                var rest = k[prefix.Length..];
                if (!recursive && rest.Contains(Path.DirectorySeparatorChar))
                {
                    return false;
                }

                return matcher(Path.GetFileName(k));
            })
            .ToArray();
    }

    public void CopyDirectory(string sourceDirectory, string targetDirectory, bool overwrite)
    {
        var src = Normalize(sourceDirectory) + Path.DirectorySeparatorChar;
        foreach (var (path, content) in _files.Where(kv => kv.Key.StartsWith(src, StringComparison.Ordinal)).ToArray())
        {
            var rel = path[src.Length..];
            var dest = Path.Combine(Normalize(targetDirectory), rel);
            if (!_files.ContainsKey(dest) || overwrite)
            {
                _files[dest] = content;
                AddParentDirectories(dest);
            }
        }
    }

    private void AddParentDirectories(string path)
    {
        var dir = Path.GetDirectoryName(path);
        while (!string.IsNullOrEmpty(dir))
        {
            if (!_directories.Add(dir))
            {
                return;
            }
            dir = Path.GetDirectoryName(dir);
        }
    }

    private static Func<string, bool> BuildMatcher(string searchPattern)
    {
        if (searchPattern == "*")
        {
            return _ => true;
        }

        if (searchPattern.StartsWith("*.", StringComparison.Ordinal))
        {
            var ext = searchPattern[1..];
            return name => name.EndsWith(ext, StringComparison.OrdinalIgnoreCase);
        }

        return name => string.Equals(name, searchPattern, StringComparison.OrdinalIgnoreCase);
    }

    private static string Normalize(string path)
        => path.Replace('/', Path.DirectorySeparatorChar).Replace('\\', Path.DirectorySeparatorChar);
}
