using AiSetup.Exceptions;
using AiSetup.Models;

namespace AiSetup.Platform;

/// <summary>
/// Default <see cref="IPathProvider"/> using <see cref="Environment.SpecialFolder"/> to resolve
/// OS-specific local paths.
/// </summary>
public sealed class PathProvider : IPathProvider
{
    /// <inheritdoc />
    public string GetLocalRoot(DeployTarget target)
    {
        return target switch
        {
            DeployTarget.ClaudeCode => GetClaudeCodeLocalRoot(),
            DeployTarget.CopilotCli => GetCopilotCliLocalRoot(),
            _ => throw new AiSetupException($"Unknown deploy target '{target}'.")
        };
    }

    private static string GetClaudeCodeLocalRoot()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".claude");
    }

    private static string GetCopilotCliLocalRoot()
    {
        var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(home, ".copilot");
    }
}
