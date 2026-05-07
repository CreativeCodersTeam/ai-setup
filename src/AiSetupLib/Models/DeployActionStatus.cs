namespace AiSetup.Models;

/// <summary>
/// Outcome classification for a planned deploy action.
/// </summary>
public enum DeployActionStatus
{
    /// <summary>Target does not exist; will be created.</summary>
    Create,

    /// <summary>Target exists and would be overwritten.</summary>
    Overwrite,

    /// <summary>Action skipped (e.g. existing file without --force).</summary>
    Skip,

    /// <summary>Action failed during planning or execution.</summary>
    Error
}
