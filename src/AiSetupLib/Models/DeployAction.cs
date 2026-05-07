namespace AiSetup.Lib.Models;

/// <summary>
/// A single planned write or copy step produced by a deploy target.
/// </summary>
/// <param name="Kind">What happens with the target.</param>
/// <param name="SourcePath">Optional source path on disk (null for aggregated content).</param>
/// <param name="TargetPath">Absolute destination path.</param>
/// <param name="Content">Materialised content to write (null when copying a folder).</param>
/// <param name="IsDirectoryCopy">True when an entire directory is copied recursively.</param>
/// <param name="Reason">Human-readable explanation, e.g. error detail or skip reason.</param>
public sealed record DeployAction(
    DeployActionKind Kind,
    string? SourcePath,
    string TargetPath,
    string? Content,
    bool IsDirectoryCopy,
    string? Reason);
