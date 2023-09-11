namespace XO.Console.Cli;

/// <summary>
/// Configures a command to be added automatically when configuring a command-line application.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public sealed class CommandAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandAttribute"/>.
    /// </summary>
    /// <param name="verbs">The sequence of verbs that invoke the command (including its parent commands).</param>
    public CommandAttribute(params string[] verbs)
    {
        Verbs = verbs;
    }

    /// <summary>
    /// An array of aliases that invoke the command.
    /// </summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Sets whether the command is hidden from generated help.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// The sequence of verbs that invoke the command.
    /// </summary>
    public string[] Verbs { get; }
}
