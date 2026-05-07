using System.Runtime.InteropServices;
using AiSetup.Exceptions;
using AiSetup.Models;
using AiSetup.Platform;

namespace AiSetup.Tests.Platform;

public sealed class PathProviderTests
{
    [Fact]
    public void GetLocalRoot_ClaudeCode_ReturnsHomeDotClaude()
    {
        var sut = new PathProvider();

        var result = sut.GetLocalRoot(DeployTarget.ClaudeCode);

        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        result.Should().Be(Path.Combine(home, ".claude"));
    }

    [Fact]
    public void GetLocalRoot_CopilotCli_ReturnsPlatformAppropriatePath()
    {
        var sut = new PathProvider();

        var result = sut.GetLocalRoot(DeployTarget.CopilotCli);

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
    public void GetLocalRoot_UnknownTarget_Throws()
    {
        var sut = new PathProvider();

        Action act = () => sut.GetLocalRoot((DeployTarget)999);

        act.Should().Throw<AiSetupException>();
    }
}
