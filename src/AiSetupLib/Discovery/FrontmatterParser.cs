using AiSetup.Lib.Exceptions;
using CreativeCoders.Core;
using YamlDotNet.Serialization;

namespace AiSetup.Lib.Discovery;

/// <inheritdoc cref="IFrontmatterParser"/>
public sealed class FrontmatterParser : IFrontmatterParser
{
    private const string Delimiter = "---";

    private readonly IDeserializer _deserializer = new DeserializerBuilder().Build();

    /// <inheritdoc />
    public FrontmatterParseResult Parse(string rawContent, string filePath)
    {
        Ensure.NotNull(rawContent, nameof(rawContent));
        Ensure.IsNotNullOrWhitespace(filePath, nameof(filePath));

        var normalised = rawContent.Replace("\r\n", "\n", StringComparison.Ordinal);
        var lines = normalised.Split('\n');

        if (lines.Length == 0 || lines[0].Trim() != Delimiter)
        {
            return new FrontmatterParseResult(
                new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase),
                rawContent,
                HasFrontmatter: false);
        }

        var endIndex = -1;

        for (var i = 1; i < lines.Length; i++)
        {
            if (lines[i].Trim() == Delimiter)
            {
                endIndex = i;

                break;
            }
        }

        if (endIndex < 0)
        {
            throw new InvalidFrontmatterException(filePath, "missing closing '---' delimiter.");
        }

        var yamlBlock = string.Join('\n', lines, 1, endIndex - 1);
        var body = endIndex + 1 < lines.Length
            ? string.Join('\n', lines, endIndex + 1, lines.Length - endIndex - 1)
            : string.Empty;

        Dictionary<string, object?> map;

        try
        {
            map = _deserializer.Deserialize<Dictionary<string, object?>>(yamlBlock)
                ?? new Dictionary<string, object?>(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex) when (ex is not InvalidFrontmatterException)
        {
            throw new InvalidFrontmatterException(filePath, ex.Message, ex);
        }

        var caseInsensitive = new Dictionary<string, object?>(map, StringComparer.OrdinalIgnoreCase);

        return new FrontmatterParseResult(caseInsensitive, body.TrimStart('\n'), HasFrontmatter: true);
    }
}
