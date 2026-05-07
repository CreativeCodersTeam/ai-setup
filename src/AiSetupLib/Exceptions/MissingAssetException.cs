namespace AiSetup.Lib.Exceptions;

/// <summary>
/// Thrown when a referenced asset cannot be located. Provides similar candidate
/// names to aid the user.
/// </summary>
public sealed class MissingAssetException : AiSetupException
{
    /// <summary>The asset name that could not be resolved.</summary>
    public string AssetName { get; }

    /// <summary>Suggestions of similar known asset names.</summary>
    public IReadOnlyList<string> Suggestions { get; }

    /// <summary>Initialises a new instance.</summary>
    public MissingAssetException(string assetName, IReadOnlyList<string> suggestions)
        : base(BuildMessage(assetName, suggestions))
    {
        AssetName = assetName;
        Suggestions = suggestions;
    }

    private static string BuildMessage(string assetName, IReadOnlyList<string> suggestions)
    {
        if (suggestions.Count == 0)
        {
            return $"Asset '{assetName}' was not found.";
        }

        return $"Asset '{assetName}' was not found. Did you mean: {string.Join(", ", suggestions)}?";
    }
}
