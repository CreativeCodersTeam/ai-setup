namespace AiSetup.Lib.Models;

/// <summary>
/// Kind of a single planned deploy action.
/// </summary>
public enum DeployActionKind
{
    /// <summary>Target file/folder does not exist and will be created.</summary>
    Create,

    /// <summary>Target file/folder exists and will be overwritten.</summary>
    Overwrite,

    /// <summary>Target exists and will be skipped (no force flag).</summary>
    Skip,

    /// <summary>The action could not be planned because of an error.</summary>
    Error,
}
