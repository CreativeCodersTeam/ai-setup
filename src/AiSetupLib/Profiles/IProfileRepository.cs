using AiSetup.Models;

namespace AiSetup.Profiles;

/// <summary>
/// Loads named profiles from a profiles directory.
/// </summary>
public interface IProfileRepository
{
    /// <summary>Returns a profile by name, or null if not present.</summary>
    /// <param name="name">Profile identifier (file stem).</param>
    Profile? Find(string name);

    /// <summary>Returns every profile that was discoverable.</summary>
    IReadOnlyList<Profile> All();
}
