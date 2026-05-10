namespace AiSetup.Models;

/// <summary>
/// Where the deploy result for an individual asset is written. Chosen per asset in the
/// profile (via the optional <c>@repo</c> / <c>@local</c> suffix on a profile entry).
/// </summary>
public enum DeployMode
{
    /// <summary>Write into a target repository (e.g. .github/, .claude/).</summary>
    Repo,

    /// <summary>Write into the user's local machine settings folder.</summary>
    Local
}
