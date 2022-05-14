namespace XO.Console.Cli;

/// <summary>
/// Configures a class as a command-line command.
/// </summary>
/// <remarks>
/// This attribute is used for reflection-based command discovery and to declare the command's verb. If a command is
/// to be configured in code, with an explicit verb, <see cref="CommandAttribute"/> is not required.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// Creates a new instance of <see cref="CommandAttribute"/>.
    /// </summary>
    /// <param name="verb">The verb that invokes the command.</param>
    public CommandAttribute(string verb)
    {
        this.Verb = verb;
    }

    /// <summary>
    /// Gets the verb that invokes the command.
    /// </summary>
    public string Verb { get; }
}
