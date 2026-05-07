namespace AiSetup.Lib.Models;

/// <summary>
/// Specifies whether assets are deployed into a project repository or into the
/// user's local AI configuration directory.
/// </summary>
public enum DeployMode
{
    /// <summary>Deploy into a target repository (e.g. <c>.github/</c> or <c>.claude/</c>).</summary>
    Repo,

    /// <summary>Deploy into the user's local configuration directory.</summary>
    Local,
}
