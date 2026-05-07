namespace AiSetup.Lib.Models;

/// <summary>
/// Optional metadata for an asset folder, stored as <c>manifest.yaml</c>.
/// </summary>
/// <param name="Group">Logical group / category name.</param>
/// <param name="Description">Human-readable description.</param>
/// <param name="DefaultTargets">Default target systems for assets in the folder.</param>
public sealed record Manifest(
    string? Group,
    string? Description,
    IReadOnlyList<DeployTarget> DefaultTargets);
