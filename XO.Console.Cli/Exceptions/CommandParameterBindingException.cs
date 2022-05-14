namespace XO.Console.Cli;

/// <summary>
/// The exception that is thrown when command-line arguments cannot be bound to a command.
/// </summary>
public class CommandParameterBindingException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandParameterBindingException"/>.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, if any.</param>
    public CommandParameterBindingException(string message, Exception? innerException = null)
        : base(message, innerException) { }
}
