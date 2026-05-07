using System.Runtime.InteropServices;
using AiSetup.Lib.Configuration;
using AiSetup.Lib.Models;
using CreativeCoders.Core;
using Microsoft.Extensions.Options;

namespace AiSetup.Lib.Targets.Platform;

/// <inheritdoc cref="IPlatformPathProvider"/>
public sealed class PlatformPathProvider : IPlatformPathProvider
{
    private readonly PathOptions _options;

    /// <summary>Initialises a new instance.</summary>
    public PlatformPathProvider(IOptions<PathOptions> options)
    {
        Ensure.NotNull(options, nameof(options));
        _options = options.Value;
    }

    /// <inheritdoc />
    public string GetLocalRoot(DeployTarget target)
    {
        var raw = target switch
        {
            DeployTarget.CopilotCli => SelectCopilotPath(),
            DeployTarget.ClaudeCode => _options.ClaudeCodeHome,
            _ => throw new ArgumentOutOfRangeException(nameof(target), target, "Unknown deploy target."),
        };

        return ExpandPath(raw);
    }

    private string SelectCopilotPath()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return _options.CopilotCliWindows;
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return _options.CopilotCliMacOs;
        }

        return _options.CopilotCliLinux;
    }

    private static string ExpandPath(string raw)
    {
        var expanded = Environment.ExpandEnvironmentVariables(raw);

        if (expanded.StartsWith("~/", StringComparison.Ordinal) || expanded == "~")
        {
            var home = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            expanded = expanded == "~"
                ? home
                : Path.Combine(home, expanded[2..]);
        }

        return expanded;
    }
}
