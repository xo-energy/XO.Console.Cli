using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// The exception that is thrown when binding a <see cref="CommandParseResult"/> containing errors.
/// </summary>
public class CommandParsingException : Exception
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandParsingException"/>.
    /// </summary>
    /// <param name="parse">The <see cref="CommandParseResult"/> that caused this exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception, if any.</param>
    public CommandParsingException(CommandParseResult parse, Exception? innerException = null)
        : base(parse.ToExceptionMessage(), innerException)
    {
        this.ParseResult = parse;
    }

    /// <summary>
    /// Gets the <see cref="CommandParseResult"/> that caused this exception.
    /// </summary>
    public CommandParseResult ParseResult { get; }
}
