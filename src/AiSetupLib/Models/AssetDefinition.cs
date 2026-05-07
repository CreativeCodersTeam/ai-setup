namespace AiSetup.Models;

/// <summary>
/// Base record describing an asset discovered in the asset repository.
/// </summary>
/// <param name="Id">Stable identifier (relative path without extension, e.g. "csharp/dotnet-tester").</param>
/// <param name="Type">The asset category.</param>
/// <param name="Name">Human-readable name from the frontmatter.</param>
/// <param name="Description">Short description from the frontmatter.</param>
/// <param name="Tags">Tags from the frontmatter, used for filtering.</param>
/// <param name="Targets">Target systems this asset is intended for. Empty list = all targets.</param>
/// <param name="SourcePath">Absolute path to the source file (or skill folder).</param>
/// <param name="ApplyTo">Optional file glob (instruction-style) the asset applies to.</param>
/// <param name="Frontmatter">Raw frontmatter values for advanced consumers.</param>
/// <param name="Body">Content of the asset without the frontmatter block.</param>
public record AssetDefinition(
    string Id,
    AssetType Type,
    string Name,
    string Description,
    IReadOnlyList<string> Tags,
    IReadOnlyList<DeployTarget> Targets,
    string SourcePath,
    string? ApplyTo,
    IReadOnlyDictionary<string, object?> Frontmatter,
    string Body);
