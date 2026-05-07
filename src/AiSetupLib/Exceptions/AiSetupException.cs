namespace AiSetup.Lib.Exceptions;

/// <summary>
/// Base exception for all errors raised by AiSetup.
/// </summary>
public class AiSetupException : Exception
{
    /// <summary>Initialises a new instance with the given message.</summary>
    public AiSetupException(string message)
        : base(message)
    {
    }

    /// <summary>Initialises a new instance with the given message and inner exception.</summary>
    public AiSetupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
