namespace AiSetup.Models;

/// <summary>
/// Determines how <see cref="Aggregation.IMcpConfigMerger"/> handles an incoming MCP server entry
/// whose name already exists in the target settings file.
/// </summary>
public enum McpConflictResolution
{
    /// <summary>Throw an <see cref="Exceptions.AiSetupException"/> on the first conflict (default).</summary>
    Fail,

    /// <summary>Replace the existing entry with the incoming one.</summary>
    Overwrite,

    /// <summary>Keep the existing entry and silently drop the incoming one.</summary>
    Skip
}
