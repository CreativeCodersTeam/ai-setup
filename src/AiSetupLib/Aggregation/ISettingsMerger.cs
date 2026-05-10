using AiSetup.Models;

namespace AiSetup.Aggregation;

/// <summary>
/// Deep-merges target-specific settings fragments (<see cref="AssetType.Settings"/>) into a
/// target's JSON settings file.
/// </summary>
public interface ISettingsMerger
{
    /// <summary>
    /// Produces the merged JSON content by applying each settings fragment, in order, onto the base document.
    /// </summary>
    /// <param name="settings">Settings assets to apply (each body must be a JSON object).</param>
    /// <param name="existingJson">Existing JSON content of the target settings file, or <see langword="null"/> if it does not exist.</param>
    /// <param name="conflictResolution">Strategy for an incoming scalar value that conflicts with an existing value.</param>
    /// <returns>UTF-8 JSON string with two-space indent.</returns>
    /// <exception cref="System.ArgumentNullException"><paramref name="settings"/> is <see langword="null"/>.</exception>
    /// <exception cref="Exceptions.AiSetupException">
    /// <paramref name="existingJson"/> is not a valid JSON object, a fragment body is not a JSON object,
    /// an asset is not a <see cref="AssetType.Settings"/> asset, or a scalar conflict occurs while
    /// <paramref name="conflictResolution"/> is <see cref="McpConflictResolution.Fail"/>.
    /// </exception>
    string Merge(
        IReadOnlyList<AssetDefinition> settings,
        string? existingJson,
        McpConflictResolution conflictResolution);
}
