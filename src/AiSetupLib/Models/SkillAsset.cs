namespace AiSetup.Models;

/// <summary>
/// Skill asset with folder structure (SKILL.md plus optional reference files).
/// </summary>
/// <param name="Id">Stable identifier (relative path without extension).</param>
/// <param name="Name">Human-readable name from the frontmatter.</param>
/// <param name="Description">Short single-line plain-text description; Markdown is not interpreted.</param>
/// <param name="Tags">Tags from the frontmatter.</param>
/// <param name="Targets">Target systems this skill is intended for.</param>
/// <param name="SourcePath">Absolute path to the SKILL.md file.</param>
/// <param name="ApplyTo">Optional file glob (instruction-style) the asset applies to.</param>
/// <param name="Frontmatter">Raw frontmatter values.</param>
/// <param name="Body">Content of SKILL.md without the frontmatter block.</param>
/// <param name="Folder">Absolute path to the skill folder.</param>
/// <param name="Files">Absolute paths to all files in the skill folder, relative to <see cref="Folder"/>.</param>
public sealed record SkillAsset(
    string Id,
    string Name,
    string Description,
    IReadOnlyList<string> Tags,
    IReadOnlyList<DeployTarget> Targets,
    string SourcePath,
    string? ApplyTo,
    IReadOnlyDictionary<string, object?> Frontmatter,
    string Body,
    string Folder,
    IReadOnlyList<string> Files)
    : AssetDefinition(Id, AssetType.Skill, Name, Description, Tags, Targets, SourcePath, ApplyTo, Frontmatter, Body);
