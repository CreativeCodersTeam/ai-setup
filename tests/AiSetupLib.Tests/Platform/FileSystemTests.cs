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
    public void WriteAllText_WhenParentMissing_CreatesParentDirectories()
    {
        // Arrange
        var sut = new FileSystem();
        var target = Path.Combine(_root, "a", "b", "c.txt");

        // Act
        sut.WriteAllText(target, "hello");

        // Assert
        File.ReadAllText(target).Should().Be("hello");
    }

    [Fact]
    public void CopyDirectory_WithNestedContent_CopiesAllNestedFiles()
    {
        // Arrange
        var sut = new FileSystem();
        var source = Path.Combine(_root, "src");
        var target = Path.Combine(_root, "dst");
        Directory.CreateDirectory(Path.Combine(source, "nested"));
        File.WriteAllText(Path.Combine(source, "top.txt"), "top");
        File.WriteAllText(Path.Combine(source, "nested", "deep.txt"), "deep");

        // Act
        sut.CopyDirectory(source, target);

        // Assert
        File.ReadAllText(Path.Combine(target, "top.txt")).Should().Be("top");
        File.ReadAllText(Path.Combine(target, "nested", "deep.txt")).Should().Be("deep");
    }

    [Fact]
    public void EnumerateFilesRecursive_OnExistingDirectory_ReturnsRelativePaths()
    {
        // Arrange
        var sut = new FileSystem();
        var dir = Path.Combine(_root, "dir");
        Directory.CreateDirectory(Path.Combine(dir, "x"));
        File.WriteAllText(Path.Combine(dir, "a.txt"), "a");
        File.WriteAllText(Path.Combine(dir, "x", "b.txt"), "b");

        // Act
        var files = sut.EnumerateFilesRecursive(dir);

        // Assert
        files.Should().BeEquivalentTo(new[] { "a.txt", Path.Combine("x", "b.txt") });
    }

    [Fact]
    public void EnumerateFilesRecursive_OnMissingDirectory_ReturnsEmpty()
    {
        // Arrange
        var sut = new FileSystem();

        // Act
        var result = sut.EnumerateFilesRecursive(Path.Combine(_root, "missing"));

        // Assert
        result.Should().BeEmpty();
    }

    [Fact]
    public void FileExists_WithExistingAndMissingFile_ReflectsActualState()
    {
        // Arrange
        var sut = new FileSystem();
        var file = Path.Combine(_root, "f.txt");

        // Act + Assert
        sut.FileExists(file).Should().BeFalse();

        File.WriteAllText(file, "x");
        sut.FileExists(file).Should().BeTrue();
    }

    [Fact]
    public void DirectoryExists_WithExistingAndMissingDirectory_ReflectsActualState()
    {
        // Arrange
        var sut = new FileSystem();
        var dir = Path.Combine(_root, "newdir");

        // Act + Assert
        sut.DirectoryExists(dir).Should().BeFalse();

        Directory.CreateDirectory(dir);
        sut.DirectoryExists(dir).Should().BeTrue();
    }

    [Fact]
    public void ReadAllText_OnExistingFile_ReturnsFileContent()
    {
        // Arrange
        var sut = new FileSystem();
        var file = Path.Combine(_root, "r.txt");
        File.WriteAllText(file, "hello world");

        // Act
        var content = sut.ReadAllText(file);

        // Assert
        content.Should().Be("hello world");
    }

    [Fact]
    public void CopyFile_WhenTargetParentMissing_CreatesParentDirectories()
    {
        // Arrange
        var sut = new FileSystem();
        var source = Path.Combine(_root, "src.txt");
        var target = Path.Combine(_root, "deep", "nested", "dst.txt");
        File.WriteAllText(source, "payload");

        // Act
        sut.CopyFile(source, target);

        // Assert
        File.ReadAllText(target).Should().Be("payload");
    }

    [Fact]
    public void CopyFile_WhenTargetExists_OverwritesExistingTarget()
    {
        // Arrange
        var sut = new FileSystem();
        var source = Path.Combine(_root, "s.txt");
        var target = Path.Combine(_root, "t.txt");
        File.WriteAllText(source, "new");
        File.WriteAllText(target, "old");

        // Act
        sut.CopyFile(source, target);

        // Assert
        File.ReadAllText(target).Should().Be("new");
    }

    [Fact]
    public void CreateDirectory_WithNestedPath_CreatesEntireHierarchy()
    {
        // Arrange
        var sut = new FileSystem();
        var nested = Path.Combine(_root, "a", "b", "c");

        // Act
        sut.CreateDirectory(nested);

        // Assert
        Directory.Exists(nested).Should().BeTrue();
    }

    [Fact]
    public void EnumerateDirectories_OnExistingPath_ReturnsImmediateChildrenOnly()
    {
        // Arrange
        var sut = new FileSystem();
        var dir = Path.Combine(_root, "parent");
        Directory.CreateDirectory(Path.Combine(dir, "child1"));
        Directory.CreateDirectory(Path.Combine(dir, "child2", "grandchild"));

        // Act
        var result = sut.EnumerateDirectories(dir);

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(Path.Combine(dir, "child1"));
        result.Should().Contain(Path.Combine(dir, "child2"));
    }

    [Fact]
    public void EnumerateDirectories_OnMissingPath_ReturnsEmpty()
    {
        // Arrange
        var sut = new FileSystem();

        // Act
        var result = sut.EnumerateDirectories(Path.Combine(_root, "missing"));

        // Assert
        result.Should().BeEmpty();
    }
}
