namespace AiSetup.Lib.Models;

/// <summary>
/// Categorises an AI asset definition.
/// </summary>
public enum AssetType
{
    /// <summary>Coding instructions written into instruction files.</summary>
    Instruction,

    /// <summary>Skill definitions with optional folder layout (SKILL.md + references).</summary>
    Skill,

    /// <summary>Agent definitions referencing skills.</summary>
    Agent,

    /// <summary>MCP server connection configuration.</summary>
    McpConfig,
}
