namespace XO.Console.Cli;

/// <summary>
/// Represents the standard input, output, and error streams for console applications.
/// </summary>
public interface IConsole
{
    /// <summary>
    /// Gets the standard input stream.
    /// </summary>
    public TextReader Input { get; }

    /// <summary>
    /// Gets the standard output stream.
    /// </summary>
    public TextWriter Output { get; }

    /// <summary>
    /// Gets the standard error stream.
    /// </summary>
    public TextWriter Error { get; }

    /// <inheritdoc cref="System.Console.IsInputRedirected"/>
    bool IsInputRedirected { get; }

    /// <inheritdoc cref="System.Console.IsOutputRedirected"/>
    bool IsOutputRedirected { get; }

    /// <inheritdoc cref="System.Console.IsErrorRedirected"/>
    bool IsErrorRedirected { get; }
}
