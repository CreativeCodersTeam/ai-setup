using AiSetupLib;
using AiSetupLib.Deploy;
using AiSetupLib.Discovery;
using AiSetupLib.Profiles;
using AiSetupLib.Targets;
using Microsoft.Extensions.DependencyInjection;

namespace AiSetupLib.Tests;

public class AiSetupServicesTests
{
    [Fact]
    public void AddAiSetup_registers_all_required_services()
    {
        var services = new ServiceCollection();
        services.AddAiSetup(repoRoot: "/repo");

        var sp = services.BuildServiceProvider();

        sp.GetService<IAssetDiscovery>().Should().NotBeNull();
        sp.GetService<IProfileResolver>().Should().NotBeNull();
        sp.GetService<IDeployService>().Should().NotBeNull();
        sp.GetService<TargetRegistry>().Should().NotBeNull();
        sp.GetServices<IDeployTarget>().Should().HaveCount(2);
    }
}
