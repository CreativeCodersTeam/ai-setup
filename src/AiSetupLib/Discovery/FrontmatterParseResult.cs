namespace AiSetup.Discovery;

/// <summary>
/// Outcome of parsing a Markdown document with optional YAML frontmatter.
/// </summary>
/// <param name="Values">Frontmatter key/value pairs (empty if no frontmatter or parse error).</param>
/// <param name="Body">The Markdown content with the frontmatter block removed.</param>
/// <param name="Warning">Warning produced by a malformed frontmatter block, otherwise <c>null</c>.</param>
public sealed record FrontmatterParseResult(
    IReadOnlyDictionary<string, object?> Values,
    string Body,
    string? Warning);
