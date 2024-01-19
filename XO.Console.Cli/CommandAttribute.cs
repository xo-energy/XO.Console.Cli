namespace XO.Console.Cli;

/// <summary>
/// Adds a command to the application.
/// </summary>
/// <remarks>
/// Declaratively configures the command implemented by the target class. Creating an instance of <see
/// cref="CommandAppBuilder"/> will automatically add this command to the application. To create sub-commands, declare a
/// custom subclass of <see cref="CommandAttribute"/> (see <see cref="CommandBranchAttribute"/>).
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class CommandAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandAttribute"/>.
    /// </summary>
    /// <param name="verb">The verb that invokes the command.</param>
    public CommandAttribute(string verb)
    {
        if (String.IsNullOrWhiteSpace(verb))
            throw new ArgumentException("must not be null or empty", nameof(verb));

        Verb = verb;
    }

    /// <summary>
    /// An array of aliases that invoke the command.
    /// </summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Describes the command's purpose in generated help.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Sets whether the command is hidden from generated help.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// The verb that invokes the command.
    /// </summary>
    public string Verb { get; }
}
