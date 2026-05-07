namespace AiSetup.Lib.Exceptions;

/// <summary>
/// Thrown when an asset's YAML frontmatter cannot be parsed or is missing
/// required fields.
/// </summary>
public sealed class InvalidFrontmatterException : AiSetupException
{
    /// <summary>The path of the offending file.</summary>
    public string FilePath { get; }

    /// <summary>Initialises a new instance.</summary>
    public InvalidFrontmatterException(string filePath, string message)
        : base($"Invalid frontmatter in '{filePath}': {message}")
    {
        FilePath = filePath;
    }

    /// <summary>Initialises a new instance with an inner exception.</summary>
    public InvalidFrontmatterException(string filePath, string message, Exception innerException)
        : base($"Invalid frontmatter in '{filePath}': {message}", innerException)
    {
        FilePath = filePath;
    }
}
