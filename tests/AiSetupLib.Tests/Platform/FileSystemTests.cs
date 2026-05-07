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

    [Fact]
    public void FileExists_ReflectsActualFileState()
    {
        var sut = new FileSystem();
        var file = Path.Combine(_root, "f.txt");

        sut.FileExists(file).Should().BeFalse();

        File.WriteAllText(file, "x");
        sut.FileExists(file).Should().BeTrue();
    }

    [Fact]
    public void DirectoryExists_ReflectsActualDirectoryState()
    {
        var sut = new FileSystem();
        var dir = Path.Combine(_root, "newdir");

        sut.DirectoryExists(dir).Should().BeFalse();

        Directory.CreateDirectory(dir);
        sut.DirectoryExists(dir).Should().BeTrue();
    }

    [Fact]
    public void ReadAllText_ReturnsFileContent()
    {
        var sut = new FileSystem();
        var file = Path.Combine(_root, "r.txt");
        File.WriteAllText(file, "hello world");

        sut.ReadAllText(file).Should().Be("hello world");
    }

    [Fact]
    public void CopyFile_CreatesParentDirectories()
    {
        var sut = new FileSystem();
        var source = Path.Combine(_root, "src.txt");
        var target = Path.Combine(_root, "deep", "nested", "dst.txt");
        File.WriteAllText(source, "payload");

        sut.CopyFile(source, target);

        File.ReadAllText(target).Should().Be("payload");
    }

    [Fact]
    public void CopyFile_OverwritesExistingTarget()
    {
        var sut = new FileSystem();
        var source = Path.Combine(_root, "s.txt");
        var target = Path.Combine(_root, "t.txt");
        File.WriteAllText(source, "new");
        File.WriteAllText(target, "old");

        sut.CopyFile(source, target);

        File.ReadAllText(target).Should().Be("new");
    }

    [Fact]
    public void CreateDirectory_CreatesNestedHierarchy()
    {
        var sut = new FileSystem();
        var nested = Path.Combine(_root, "a", "b", "c");

        sut.CreateDirectory(nested);

        Directory.Exists(nested).Should().BeTrue();
    }

    [Fact]
    public void EnumerateDirectories_ReturnsImmediateChildrenOnly()
    {
        var sut = new FileSystem();
        var dir = Path.Combine(_root, "parent");
        Directory.CreateDirectory(Path.Combine(dir, "child1"));
        Directory.CreateDirectory(Path.Combine(dir, "child2", "grandchild"));

        var result = sut.EnumerateDirectories(dir);

        result.Should().HaveCount(2);
        result.Should().Contain(Path.Combine(dir, "child1"));
        result.Should().Contain(Path.Combine(dir, "child2"));
    }

    [Fact]
    public void EnumerateDirectories_MissingPath_ReturnsEmpty()
    {
        var sut = new FileSystem();

        sut.EnumerateDirectories(Path.Combine(_root, "missing")).Should().BeEmpty();
    }
}
