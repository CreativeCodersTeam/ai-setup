namespace AiSetup.Exceptions;

/// <summary>
/// Raised when a referenced profile name cannot be resolved.
/// </summary>
public sealed class MissingProfileException : AiSetupException
{
    /// <summary>Initializes a new instance.</summary>
    /// <param name="profileName">The profile that was requested.</param>
    /// <param name="suggestions">Closest existing profile names.</param>
    public MissingProfileException(string profileName, IReadOnlyList<string> suggestions)
        : base(BuildMessage(profileName, suggestions))
    {
        ProfileName = profileName;
        Suggestions = suggestions;
    }

    /// <summary>The profile name that could not be resolved.</summary>
    public string ProfileName { get; }

    /// <summary>Closest existing profile names ordered by similarity.</summary>
    public IReadOnlyList<string> Suggestions { get; }

    private static string BuildMessage(string profileName, IReadOnlyList<string> suggestions)
    {
        var hint = suggestions.Count == 0
            ? string.Empty
            : $" Did you mean: {string.Join(", ", suggestions)}?";

        return $"Profile '{profileName}' was not found.{hint}";
    }
}
