using AiSetup.Lib.Models;

namespace AiSetup.Lib.Profiles;

/// <summary>
/// Resolves profiles from a source repository's <c>profiles/</c> folder.
/// </summary>
public interface IProfileResolver
{
    /// <summary>Lists every profile present under <c>profiles/</c>.</summary>
    IReadOnlyList<Profile> ListProfiles(string repoRoot);

    /// <summary>Loads a single profile by name. Returns null when not found.</summary>
    Profile? LoadProfile(string repoRoot, string name);
}
