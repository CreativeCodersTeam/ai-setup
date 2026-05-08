using AiSetup.Cli.Infrastructure;
using AiSetup.Exceptions;
using AiSetup.Platform;

namespace AiSetup.Cli.Tests.Infrastructure;

public sealed class SourceRepoResolverTests
{
    [Fact]
    public void Resolve_WithExistingCandidate_ReturnsAbsolutePath()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.DirectoryExists(A<string>._)).Returns(true);

        // Act
        var result = SourceRepoResolver.Resolve(fs, "/some/path");

        // Assert
        result.Should().Be(Path.GetFullPath("/some/path"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithBlankCandidate_FallsBackToCurrentDirectory(string? candidate)
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.DirectoryExists(A<string>._)).Returns(true);

        // Act
        var result = SourceRepoResolver.Resolve(fs, candidate);

        // Assert
        result.Should().Be(Path.GetFullPath(Environment.CurrentDirectory));
    }

    [Fact]
    public void Resolve_WhenDirectoryDoesNotExist_ThrowsAiSetupException()
    {
        // Arrange
        var fs = A.Fake<IFileSystem>();
        A.CallTo(() => fs.DirectoryExists(A<string>._)).Returns(false);

        // Act
        Action act = () => SourceRepoResolver.Resolve(fs, "/missing");

        // Assert
        act.Should().Throw<AiSetupException>().WithMessage("*does not exist*");
    }

    [Fact]
    public void Resolve_WithNullFileSystem_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => SourceRepoResolver.Resolve(null!, "/x");

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
