namespace AiSetupLib.Models;

public sealed record Profile(
    string Name,
    string Description,
    IReadOnlyList<string> Agents,
    IReadOnlyList<string> Instructions,
    IReadOnlyList<string> Skills,
    IReadOnlyList<string> McpConfigs);
