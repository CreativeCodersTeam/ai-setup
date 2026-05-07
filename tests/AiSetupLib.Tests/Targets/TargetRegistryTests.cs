using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Targets;

namespace AiSetup.Tests.Targets;

public sealed class TargetRegistryTests
{
    [Fact]
    public void Get_WithRegisteredTarget_ReturnsRegisteredInstance()
    {
        // Arrange
        var copilot = A.Fake<IDeployTarget>();
        A.CallTo(() => copilot.Target).Returns(DeployTarget.CopilotCli);
        var claude = A.Fake<IDeployTarget>();
        A.CallTo(() => claude.Target).Returns(DeployTarget.ClaudeCode);

        var sut = new TargetRegistry([copilot, claude]);

        // Act
        var copilotResult = sut.Get(DeployTarget.CopilotCli);
        var claudeResult = sut.Get(DeployTarget.ClaudeCode);

        // Assert
        copilotResult.Should().BeSameAs(copilot);
        claudeResult.Should().BeSameAs(claude);
    }

    [Fact]
    public void Get_WithUnregisteredTarget_ThrowsAiSetupException()
    {
        // Arrange
        var sut = new TargetRegistry([]);

        // Act
        Action act = () => sut.Get(DeployTarget.CopilotCli);

        // Assert
        act.Should().Throw<AiSetupException>();
    }

    [Fact]
    public void Constructor_WithDuplicateTargets_ThrowsArgumentException()
    {
        // Arrange
        var first = A.Fake<IDeployTarget>();
        A.CallTo(() => first.Target).Returns(DeployTarget.ClaudeCode);
        var second = A.Fake<IDeployTarget>();
        A.CallTo(() => second.Target).Returns(DeployTarget.ClaudeCode);

        // Act
        Action act = () => _ = new TargetRegistry([first, second]);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_WithNullTargets_ThrowsArgumentNullException()
    {
        // Act
        Action act = () => _ = new TargetRegistry(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }
}
