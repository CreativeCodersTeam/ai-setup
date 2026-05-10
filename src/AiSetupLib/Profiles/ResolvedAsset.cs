using AiSetup.Models;

namespace AiSetup.Profiles;

/// <summary>
/// A discovered asset paired with the deploy mode chosen for it in the profile.
/// </summary>
/// <param name="Definition">The resolved asset definition.</param>
/// <param name="Mode">Where the asset is written (repo or local).</param>
public sealed record ResolvedAsset(AssetDefinition Definition, DeployMode Mode);
