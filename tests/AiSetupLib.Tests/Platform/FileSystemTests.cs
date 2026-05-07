using AiSetup.Platform;

namespace AiSetup.Tests.Platform;

public sealed class FileSystemTests : IDisposable
{
    private readonly string _root;

    public FileSystemTests()
    {
        _root = Path.Combine(Path.GetTempPath(), "ai-setup-fs-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_root);
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
        {
            Directory.Delete(_root, recursive: true);
        }
    }

    [Fact]
    public void WriteAllText_CreatesParentDirectories()
    {
        var sut = new FileSystem();
        var target = Path.Combine(_root, "a", "b", "c.txt");

        sut.WriteAllText(target, "hello");

        File.ReadAllText(target).Should().Be("hello");
    }

    [Fact]
    public void CopyDirectory_CopiesNestedFiles()
    {
        var sut = new FileSystem();
        var source = Path.Combine(_root, "src");
        var target = Path.Combine(_root, "dst");
        Directory.CreateDirectory(Path.Combine(source, "nested"));
        File.WriteAllText(Path.Combine(source, "top.txt"), "top");
        File.WriteAllText(Path.Combine(source, "nested", "deep.txt"), "deep");

        sut.CopyDirectory(source, target);

        File.ReadAllText(Path.Combine(target, "top.txt")).Should().Be("top");
        File.ReadAllText(Path.Combine(target, "nested", "deep.txt")).Should().Be("deep");
    }

    [Fact]
    public void EnumerateFilesRecursive_ReturnsRelativePaths()
    {
        var sut = new FileSystem();
        var dir = Path.Combine(_root, "dir");
        Directory.CreateDirectory(Path.Combine(dir, "x"));
        File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
        File.WriteAllText(Path.Combine(dir, "x", "b.txt"), "b");

        var files = sut.EnumerateFilesRecursive(dir);

        files.Should().BeEquivalentTo(new[] { "a.txt", Path.Combine("x", "b.txt") });
    }

    [Fact]
    public void EnumerateFilesRecursive_MissingDirectory_ReturnsEmpty()
    {
        var sut = new FileSystem();
        sut.EnumerateFilesRecursive(Path.Combine(_root, "missing")).Should().BeEmpty();
    }
}
