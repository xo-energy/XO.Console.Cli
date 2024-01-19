namespace XO.Console.Cli.Model;

/// <summary>
/// Represents the parser's interpretation of a command-line argument.
/// </summary>
/// <param name="TokenType">The type of token.</param>
/// <param name="Value">The token value.</param>
/// <param name="Context">An implementation-specific value describing how the token is to be used.</param>
public sealed record CommandToken(
    CommandTokenType TokenType,
    string Value,
    object? Context = null);
