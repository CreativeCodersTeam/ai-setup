namespace AiSetup.Models;

/// <summary>
/// AI system the assets are deployed to.
/// </summary>
public enum DeployTarget
{
    /// <summary>GitHub Copilot CLI.</summary>
    CopilotCli,

    /// <summary>Anthropic Claude Code.</summary>
    ClaudeCode
}
