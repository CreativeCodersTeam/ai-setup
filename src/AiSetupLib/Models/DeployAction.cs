namespace AiSetupLib.Models;

public enum DeployActionKind
{
    Create,
    Overwrite,
    Skip,
    Error,
}

public sealed record DeployAction(
    DeployActionKind Kind,
    string TargetPath,
    IReadOnlyList<string> SourceAssets,
    string? Message = null);
