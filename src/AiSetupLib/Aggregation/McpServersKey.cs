namespace AiSetup.Aggregation;

/// <summary>
/// JSON property names used by each target system to host the MCP server map.
/// </summary>
public static class McpServersKey
{
    /// <summary>Claude Code's settings.json uses <c>mcpServers</c>.</summary>
    public const string ClaudeCode = "mcpServers";

    /// <summary>VS Code (Copilot CLI) mcp.json uses <c>servers</c>.</summary>
    public const string CopilotCli = "servers";
}
