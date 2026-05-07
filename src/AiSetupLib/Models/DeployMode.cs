namespace AiSetup.Models;

/// <summary>
/// Where the deploy result is written.
/// </summary>
public enum DeployMode
{
    /// <summary>Write into a target repository (e.g. .github/, .claude/).</summary>
    Repo,

    /// <summary>Write into the user's local machine settings folder.</summary>
    Local
}
