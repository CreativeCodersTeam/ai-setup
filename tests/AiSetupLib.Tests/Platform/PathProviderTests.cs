using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Platform;

namespace AiSetup.Tests.Platform;

public sealed class PathProviderTests
{
    [Fact]
    public void GetLocalRoot_WithClaudeCodeTarget_ReturnsHomeDotClaude()
    {
        // Arrange
        var sut = new PathProvider();

        // Act
        var result = sut.GetLocalRoot(DeployTarget.ClaudeCode);

        // Assert
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        result.Should().Be(Path.Combine(home, ".claude"));
    }

    [Fact]
    public void GetLocalRoot_WithCopilotCliTarget_ReturnsHomeDotCopilot()
    {
        // Arrange
        var sut = new PathProvider();

        // Act
        var result = sut.GetLocalRoot(DeployTarget.CopilotCli);

        // Assert
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        result.Should().Be(Path.Combine(home, ".copilot"));
    }

    [Fact]
    public void GetLocalRoot_WithUnknownTarget_ThrowsAiSetupException()
    {
        // Arrange
        var sut = new PathProvider();

        // Act
        Action act = () => sut.GetLocalRoot((DeployTarget)999);

        // Assert
        act.Should().Throw<AiSetupException>();
    }
}
