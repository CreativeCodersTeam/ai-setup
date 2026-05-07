namespace AiSetup.Lib.Models;

/// <summary>
/// Identifies the AI system that assets are deployed to.
/// </summary>
public enum DeployTarget
{
    /// <summary>GitHub Copilot CLI layout.</summary>
    CopilotCli,

    /// <summary>Anthropic Claude Code layout.</summary>
    ClaudeCode,
}
