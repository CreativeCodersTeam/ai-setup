namespace AiSetup.Exceptions;

/// <summary>
/// Base type for all exceptions raised by AiSetupLib.
/// </summary>
public class AiSetupException : Exception
{
    /// <summary>Initializes a new instance.</summary>
    public AiSetupException()
    {
    }

    /// <summary>Initializes a new instance with a message.</summary>
    /// <param name="message">Human-readable description.</param>
    public AiSetupException(string message)
        : base(message)
    {
    }

    /// <summary>Initializes a new instance with a message and inner exception.</summary>
    /// <param name="message">Human-readable description.</param>
    /// <param name="innerException">Underlying cause.</param>
    public AiSetupException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
