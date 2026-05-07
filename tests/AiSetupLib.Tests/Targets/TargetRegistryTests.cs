using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Targets;

namespace AiSetup.Tests.Targets;

public sealed class TargetRegistryTests
{
    [Fact]
    public void Get_ReturnsRegisteredTarget()
    {
        var copilot = A.Fake<IDeployTarget>();
        A.CallTo(() => copilot.Target).Returns(DeployTarget.CopilotCli);
        var claude = A.Fake<IDeployTarget>();
        A.CallTo(() => claude.Target).Returns(DeployTarget.ClaudeCode);

        var sut = new TargetRegistry([copilot, claude]);

        sut.Get(DeployTarget.CopilotCli).Should().BeSameAs(copilot);
        sut.Get(DeployTarget.ClaudeCode).Should().BeSameAs(claude);
    }

    [Fact]
    public void Get_UnregisteredTarget_Throws()
    {
        var sut = new TargetRegistry([]);

        Action act = () => sut.Get(DeployTarget.CopilotCli);

        act.Should().Throw<AiSetupException>();
    }


    [Fact]
    public void Constructor_DuplicateTargets_Throws()
    {
        var first = A.Fake<IDeployTarget>();
        A.CallTo(() => first.Target).Returns(DeployTarget.ClaudeCode);
        var second = A.Fake<IDeployTarget>();
        A.CallTo(() => second.Target).Returns(DeployTarget.ClaudeCode);

        Action act = () => _ = new TargetRegistry([first, second]);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Constructor_NullTargets_Throws()
    {
        Action act = () => _ = new TargetRegistry(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}
