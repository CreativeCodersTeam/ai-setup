using AiSetupLib.Models;

namespace AiSetupLib.Deploy;

public interface IDeployService
{
    DeployPlan Deploy(DeployOptions options);
}
