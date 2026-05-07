namespace AiSetup.Lib.Models;

/// <summary>
/// Represents a single AI asset definition discovered in the source repository.
/// </summary>
/// <param name="Name">Stable asset identifier (typically file or folder name without extension).</param>
/// <param name="Description">Human-readable summary from the asset frontmatter.</param>
/// <param name="Type">The asset category.</param>
/// <param name="Tags">Free-form tags from the frontmatter.</param>
/// <param name="Targets">Target systems this asset supports.</param>
/// <param name="ApplyTo">Optional file glob the asset applies to (e.g. <c>**/*.cs</c>).</param>
/// <param name="RelativePath">Path relative to the source repository root.</param>
/// <param name="AbsolutePath">Absolute filesystem path of the primary asset file.</param>
/// <param name="RawContent">The full raw content (including frontmatter) of the primary file.</param>
/// <param name="Body">Markdown body without frontmatter.</param>
/// <param name="Frontmatter">Parsed frontmatter as a string-keyed dictionary.</param>
public sealed record AssetDefinition(
    string Name,
    string Description,
    AssetType Type,
    IReadOnlyList<string> Tags,
    IReadOnlyList<DeployTarget> Targets,
    string? ApplyTo,
    string RelativePath,
    string AbsolutePath,
    string RawContent,
    string Body,
    IReadOnlyDictionary<string, object?> Frontmatter);
