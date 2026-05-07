namespace AiSetup.Lib.Discovery;

/// <summary>
/// Parses YAML frontmatter from markdown / YAML files.
/// </summary>
public interface IFrontmatterParser
{
    /// <summary>
    /// Splits the source into a frontmatter dictionary and the remaining body.
    /// </summary>
    /// <param name="rawContent">Full file content.</param>
    /// <param name="filePath">Path used in error messages.</param>
    FrontmatterParseResult Parse(string rawContent, string filePath);
}

/// <summary>
/// Result of <see cref="IFrontmatterParser.Parse"/>.
/// </summary>
/// <param name="Frontmatter">Parsed frontmatter values; empty when none was present.</param>
/// <param name="Body">The body that follows the frontmatter (or the entire file when no frontmatter is present).</param>
/// <param name="HasFrontmatter">True when a frontmatter block was found.</param>
public sealed record FrontmatterParseResult(
    IReadOnlyDictionary<string, object?> Frontmatter,
    string Body,
    bool HasFrontmatter);
