using AiSetupLib.Models;
using AiSetupLib.Targets;

namespace AiSetupLib.Tests.Targets;

public class TargetRegistryTests
{
    [Fact]
    public void Resolves_target_by_enum()
    {
        var copilot = A.Fake<IDeployTarget>();
        A.CallTo(() => copilot.Target).Returns(DeployTarget.CopilotCli);
        var claude = A.Fake<IDeployTarget>();
        A.CallTo(() => claude.Target).Returns(DeployTarget.ClaudeCode);

        var registry = new TargetRegistry([copilot, claude]);

        registry.Get(DeployTarget.CopilotCli).Should().BeSameAs(copilot);
        registry.Get(DeployTarget.ClaudeCode).Should().BeSameAs(claude);
    }

    [Fact]
    public void Throws_when_target_not_registered()
    {
        var registry = new TargetRegistry([]);
        var act = () => registry.Get(DeployTarget.CopilotCli);
        act.Should().Throw<InvalidOperationException>();
    }
}
