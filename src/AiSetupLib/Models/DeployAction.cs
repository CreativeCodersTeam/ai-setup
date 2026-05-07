namespace AiSetup.Models;

/// <summary>
/// Base type for a planned deploy action. Concrete subtypes describe the operation.
/// </summary>
/// <param name="TargetPath">Absolute path the action writes to.</param>
/// <param name="Status">Effect on the target (create/overwrite/skip/error).</param>
/// <param name="Description">Short human-readable summary used by dry-run rendering.</param>
public abstract record DeployAction(string TargetPath, DeployActionStatus Status, string Description);

/// <summary>
/// Action that writes a single text file (Markdown or JSON).
/// </summary>
/// <param name="TargetPath">Absolute path of the file to write.</param>
/// <param name="Content">UTF-8 file content.</param>
/// <param name="Status">Effect on the target.</param>
/// <param name="Description">Short summary for rendering.</param>
public sealed record WriteFileAction(string TargetPath, string Content, DeployActionStatus Status, string Description)
    : DeployAction(TargetPath, Status, Description);

/// <summary>
/// Action that copies a directory tree (used for skill folders with references).
/// </summary>
/// <param name="SourcePath">Absolute source directory.</param>
/// <param name="TargetPath">Absolute destination directory.</param>
/// <param name="Status">Effect on the target.</param>
/// <param name="Description">Short summary for rendering.</param>
public sealed record CopyDirectoryAction(string SourcePath, string TargetPath, DeployActionStatus Status, string Description)
    : DeployAction(TargetPath, Status, Description);

/// <summary>
/// Action that creates a backup file (e.g. CLAUDE.md.bak) before writing the destination.
/// </summary>
/// <param name="TargetPath">Absolute path of the backup file that will be produced.</param>
/// <param name="OriginalPath">Absolute path of the file being backed up.</param>
/// <param name="Status">Effect on the target.</param>
/// <param name="Description">Short summary for rendering.</param>
public sealed record BackupFileAction(string TargetPath, string OriginalPath, DeployActionStatus Status, string Description)
    : DeployAction(TargetPath, Status, Description);
