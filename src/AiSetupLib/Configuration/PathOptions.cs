namespace AiSetup.Lib.Configuration;

/// <summary>
/// Configurable local install paths per target system. Defaults are pulled from
/// the design spec and can be overridden via configuration.
/// </summary>
public sealed class PathOptions
{
    /// <summary>Local Copilot CLI directory (Windows).</summary>
    public string CopilotCliWindows { get; set; } = "%APPDATA%/GitHub Copilot CLI";

    /// <summary>Local Copilot CLI directory (macOS).</summary>
    public string CopilotCliMacOs { get; set; } = "~/Library/Application Support/github-copilot";

    /// <summary>Local Copilot CLI directory (Linux).</summary>
    public string CopilotCliLinux { get; set; } = "~/.config/github-copilot";

    /// <summary>Local Claude Code directory (all platforms).</summary>
    public string ClaudeCodeHome { get; set; } = "~/.claude";
}
