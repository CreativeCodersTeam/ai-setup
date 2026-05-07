using AiSetup.Models;

namespace AiSetup.Exceptions;

/// <summary>
/// Raised when a referenced asset ID could not be resolved against the asset repository.
/// </summary>
public sealed class MissingAssetException : AiSetupException
{
    /// <summary>Initializes a new instance.</summary>
    /// <param name="assetType">The kind of asset that was missing.</param>
    /// <param name="assetId">The ID that could not be resolved.</param>
    /// <param name="suggestions">Closest existing IDs (Levenshtein-ordered) for the user.</param>
    public MissingAssetException(AssetType assetType, string assetId, IReadOnlyList<string> suggestions)
        : base(BuildMessage(assetType, assetId, suggestions))
    {
        AssetType = assetType;
        AssetId = assetId;
        Suggestions = suggestions;
    }

    /// <summary>The kind of asset that was missing.</summary>
    public AssetType AssetType { get; }

    /// <summary>The ID that could not be resolved.</summary>
    public string AssetId { get; }

    /// <summary>Suggested similar IDs.</summary>
    public IReadOnlyList<string> Suggestions { get; }

    private static string BuildMessage(AssetType assetType, string assetId, IReadOnlyList<string> suggestions)
    {
        var hint = suggestions.Count == 0
            ? string.Empty
            : $" Did you mean: {string.Join(", ", suggestions)}?";

        return $"{assetType} '{assetId}' was not found.{hint}";
    }
}
