namespace AiSetup.Models;

/// <summary>
/// A reference to an asset within a profile, together with the deploy mode chosen for it.
/// </summary>
/// <param name="Id">Asset identifier (e.g. <c>csharp/csharp.instructions</c>).</param>
/// <param name="Mode">Where this asset is written (repo or local).</param>
public sealed record ProfileAssetRef(string Id, DeployMode Mode);
