using AiSetupLib.Models;

namespace AiSetupLib.Tests.Models;

public class EnumTests
{
    [Fact]
    public void AssetType_has_required_members()
    {
        Enum.GetNames<AssetType>().Should()
            .BeEquivalentTo("Instruction", "Skill", "Agent", "McpConfig");
    }

    [Fact]
    public void DeployTarget_has_required_members()
    {
        Enum.GetNames<DeployTarget>().Should()
            .BeEquivalentTo("CopilotCli", "ClaudeCode");
    }

    [Fact]
    public void DeployMode_has_required_members()
    {
        Enum.GetNames<DeployMode>().Should()
            .BeEquivalentTo("Repo", "Local");
    }
}
