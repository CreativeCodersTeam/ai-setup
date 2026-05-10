namespace AiSetup.Models;

/// <summary>
/// The category of an asset that can be discovered and deployed by the tool.
/// </summary>
public enum AssetType
{
    /// <summary>Coding guideline or instruction file (single Markdown document).</summary>
    Instruction,

    /// <summary>Folder-structured workflow (SKILL.md plus optional references/).</summary>
    Skill,

    /// <summary>Agent definition referencing skills and instructions.</summary>
    Agent,

    /// <summary>MCP server configuration (one server per file).</summary>
    McpConfig,

    /// <summary>Target-specific settings fragment, deep-merged into the target's settings file.</summary>
    Settings
}
