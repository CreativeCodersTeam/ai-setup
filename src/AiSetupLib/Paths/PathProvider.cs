using System.Runtime.InteropServices;
using AiSetupLib.Models;

namespace AiSetupLib.Paths;

public enum PlatformKind { Windows, MacOS, Linux }

public sealed class PathProvider : IPathProvider
{
    private readonly string _home;
    private readonly string _appData;
    private readonly PlatformKind _platform;

    public PathProvider() : this(
        home: Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        platform: DetectPlatform(),
        appData: Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData))
    { }

    public PathProvider(string home, PlatformKind? platform = null, string? appData = null)
    {
        _home = home;
        _platform = platform ?? DetectPlatform();
        _appData = appData ?? home;
    }

    public string GetLocalRoot(DeployTarget target) => target switch
    {
        DeployTarget.ClaudeCode => Combine(_home, ".claude"),
        DeployTarget.CopilotCli => _platform switch
        {
            PlatformKind.Windows => Combine(_appData, "GitHub Copilot CLI"),
            PlatformKind.MacOS => Combine(_home, "Library/Application Support/github-copilot"),
            PlatformKind.Linux => Combine(_home, ".config/github-copilot"),
            _ => throw new PlatformNotSupportedException(),
        },
        _ => throw new ArgumentOutOfRangeException(nameof(target)),
    };

    public string GetRepoSubPath(DeployTarget target, AssetType type) => (target, type) switch
    {
        (DeployTarget.CopilotCli, AssetType.Instruction) => ".github/instructions",
        (DeployTarget.CopilotCli, AssetType.Skill) => ".github/skills",
        (DeployTarget.CopilotCli, AssetType.Agent) => ".github/agents",
        (DeployTarget.CopilotCli, AssetType.McpConfig) => ".vscode",
        (DeployTarget.ClaudeCode, AssetType.Skill) => ".claude/skills",
        (DeployTarget.ClaudeCode, AssetType.McpConfig) => ".claude",
        (DeployTarget.ClaudeCode, AssetType.Instruction) => "",
        (DeployTarget.ClaudeCode, AssetType.Agent) => "",
        _ => throw new ArgumentOutOfRangeException(nameof(target)),
    };

    public string GetClaudeAggregatedFileName() => "CLAUDE.md";

    public string GetMcpSettingsRelativePath(DeployTarget target) => target switch
    {
        DeployTarget.CopilotCli => ".vscode/mcp.json",
        DeployTarget.ClaudeCode => ".claude/settings.json",
        _ => throw new ArgumentOutOfRangeException(nameof(target)),
    };

    private static string Combine(string a, string b)
    {
        if (b.Contains('/') && !b.Contains('\\'))
        {
            return a.TrimEnd('/', '\\') + "/" + b;
        }
        if (a.Contains('\\'))
        {
            return a.TrimEnd('/', '\\') + "\\" + b;
        }
        return Path.Combine(a, b);
    }

    private static PlatformKind DetectPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) return PlatformKind.Windows;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX)) return PlatformKind.MacOS;
        return PlatformKind.Linux;
    }
}
