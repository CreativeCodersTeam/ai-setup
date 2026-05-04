using AiSetupLib.Models;

namespace AiSetupLib.Profiles;

public interface IProfileResolver
{
    Profile Resolve(string repoRoot, string profileName);
}
