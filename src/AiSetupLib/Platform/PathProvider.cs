using System.Runtime.InteropServices;
using AiSetup.Exceptions;
using AiSetup.Models;

namespace AiSetup.Platform;

/// <summary>
/// Default <see cref="IPathProvider"/> using <see cref="Environment.SpecialFolder"/> and
/// <see cref="RuntimeInformation"/> to resolve OS-specific local paths.
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
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            return Path.Combine(appData, "GitHub Copilot CLI");
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            return Path.Combine(home, "Library", "Application Support", "github-copilot");
        }

        var xdgConfig = Environment.GetEnvironmentVariable("XDG_CONFIG_HOME");

        if (!string.IsNullOrEmpty(xdgConfig))
        {
            return Path.Combine(xdgConfig, "github-copilot");
        }

        var linuxHome = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        return Path.Combine(linuxHome, ".config", "github-copilot");
    }
}
