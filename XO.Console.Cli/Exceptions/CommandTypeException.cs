namespace XO.Console.Cli;

/// <summary>
/// The exception that is thrown when a command implementation or command parameters type is invalid.
/// </summary>
public class CommandTypeException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandTypeException"/>.
    /// </summary>
    /// <param name="type">The type that caused this exception.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, if any.</param>
    public CommandTypeException(Type type, string message, Exception? innerException = null)
        : base($"{type}: {message}", innerException)
    {
        this.Type = type;
    }

    /// <summary>
    /// Gets the type that caused this exception.
    /// </summary>
    public Type Type { get; }
}
