namespace AiSetupLib.Deploy;

public sealed class AssetNotFoundException : Exception
{
    public IReadOnlyList<string> MissingNames { get; }
    public IReadOnlyDictionary<string, IReadOnlyList<string>> Suggestions { get; }

    public AssetNotFoundException(
        IReadOnlyList<string> missing,
        IReadOnlyDictionary<string, IReadOnlyList<string>> suggestions)
        : base(BuildMessage(missing, suggestions))
    {
        MissingNames = missing;
        Suggestions = suggestions;
    }

    private static string BuildMessage(
        IReadOnlyList<string> missing,
        IReadOnlyDictionary<string, IReadOnlyList<string>> suggestions)
    {
        var parts = missing.Select(m =>
        {
            IReadOnlyList<string> hits = suggestions.TryGetValue(m, out var s)
                ? s
                : Array.Empty<string>();
            return hits.Count == 0
                ? $"  - {m}"
                : $"  - {m} (did you mean: {string.Join(", ", hits)}?)";
        });
        return "The following assets were not found:\n" + string.Join('\n', parts);
    }
}
