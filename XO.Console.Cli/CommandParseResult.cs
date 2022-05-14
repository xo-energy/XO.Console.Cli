using System.Collections.Immutable;
using System.Text;

namespace XO.Console.Cli;

/// <summary>
/// Represents the result of parsing a sequence of command-line arguments.
/// </summary>
public sealed class CommandParseResult
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandParseResult"/>.
    /// </summary>
    /// <param name="tokens">The tokens recognized from the command-line arguments.</param>
    /// <param name="errors">The list of errors encountered while parsing, if any.</param>
    public CommandParseResult(
        ImmutableArray<CommandToken> tokens,
        ImmutableList<string> errors)
    {
        this.Tokens = tokens;
        this.Errors = errors;
    }

    /// <summary>
    /// Gets the tokens recognized from the command-line arguments.
    /// </summary>
    public ImmutableArray<CommandToken> Tokens { get; }

    /// <summary>
    /// Gets the list of errors encountered while parsing, if any.
    /// </summary>
    public ImmutableList<string> Errors { get; }

    /// <summary>
    /// Enumerates the <see cref="Tokens"/> of type <see cref="CommandTokenType.Command"/>.
    /// </summary>
    public IEnumerable<CommandToken> GetVerbs()
    {
        foreach (var token in this.Tokens)
        {
            if (token.TokenType == CommandTokenType.Command)
                yield return token;
        }
    }

    /// <summary>
    /// Gets an exception message that describes this <see cref="CommandParseResult"/>.
    /// </summary>
    /// <returns>If there are any unknown tokens or errors, an error message; otherwise, a success message.</returns>
    public string ToExceptionMessage()
    {
        var message = new StringBuilder();
        var count = 0;

        for (int i = 0; i < this.Tokens.Length; ++i)
        {
            var token = this.Tokens[i];
            if (token.TokenType != CommandTokenType.Unknown)
                continue;

            if (count++ > 0) message.AppendLine();

            message.Append($"{token.Context}: '{token.Value}'");
        }

        foreach (var error in this.Errors)
        {
            if (count++ > 0) message.AppendLine();

            message.Append(error);
        }

        if (count > 1)
            message.Insert(0, $"Parsing failed! ({count} errors){Environment.NewLine}");

        if (count == 0)
            message.Append("Success");

        return message.ToString();
    }
}
