namespace XO.Console.Cli.Model;

/// <summary>
/// Describes the parser's interpretation of a command-line argument.
/// </summary>
public enum CommandTokenType
{
    /// <summary>
    /// A token with special meaning defined by the parser.
    /// </summary>
    System,

    /// <summary>
    /// An argument value.
    /// </summary>
    Argument,

    /// <summary>
    /// A verb specifying the command to invoke.
    /// </summary>
    Command,

    /// <summary>
    /// An option name.
    /// </summary>
    Option,

    /// <summary>
    /// A group of short option flags.
    /// </summary>
    OptionGroup,

    /// <summary>
    /// An option value.
    /// </summary>
    OptionValue,

    /// <summary>
    /// A token that could not be bound to any argument, option, or command.
    /// </summary>
    Unknown,
}
