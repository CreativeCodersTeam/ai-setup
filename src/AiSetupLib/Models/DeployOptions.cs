namespace AiSetupLib.Models;

public sealed record DeployOptions(
    DeployTarget Target,
    DeployMode Mode,
    string DestinationPath)
{
    public string? Profile { get; init; }
    public IReadOnlyList<string> Agents { get; init; } = [];
    public IReadOnlyList<string> Skills { get; init; } = [];
    public IReadOnlyList<string> Instructions { get; init; } = [];
    public IReadOnlyList<string> McpConfigs { get; init; } = [];
    public bool DryRun { get; init; }
    public bool Force { get; init; }
}
