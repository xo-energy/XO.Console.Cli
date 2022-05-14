namespace XO.Console.Cli;

/// <summary>
/// Configures a property as a command-line option.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public sealed class CommandOptionAttribute : Attribute
{
    /// <summary>
    /// Creates a new instance of <see cref="CommandOptionAttribute"/>.
    /// </summary>
    /// <param name="name">The option name, including the option leader (prefix).</param>
    /// <param name="aliases">A collection of option aliases, including the option leader (prefix).</param>
    public CommandOptionAttribute(string name, params string[] aliases)
    {
        this.Name = name;
        this.Aliases = aliases;
    }

    /// <summary>
    /// The option name, including the option leader (prefix).
    /// </summary>
    /// <example>--option</example>
    public string Name { get; }

    /// <summary>
    /// A collection of option aliases, including the option leader (prefix).
    /// </summary>
    public IReadOnlyCollection<string> Aliases { get; }

    /// <summary>
    /// Gets or sets whether this option should be hidden from generated help.
    /// </summary>
    public bool IsHidden { get; set; }
}
