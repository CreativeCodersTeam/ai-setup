using System.Runtime.InteropServices;
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
    public void GetLocalRoot_WithCopilotCliTarget_ReturnsPlatformAppropriatePath()
    {
        // Arrange
        var sut = new PathProvider();

        // Act
        var result = sut.GetLocalRoot(DeployTarget.CopilotCli);

        // Assert
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            result.Should().Be(Path.Combine(home, "Library", "Application Support", "github-copilot"));
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            result.Should().EndWith("GitHub Copilot CLI");
        }
        else
        {
            result.Should().EndWith("github-copilot");
        }
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
