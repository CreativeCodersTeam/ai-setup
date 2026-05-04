namespace AiSetupLib.Models;

public sealed record AssetDefinition(
    string Name,
    string Description,
    AssetType Type,
    IReadOnlyList<string> Tags,
    IReadOnlyList<DeployTarget> Targets,
    string? ApplyTo,
    string SourcePath,
    string Body);
