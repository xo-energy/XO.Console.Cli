using System.Collections.Immutable;

namespace XO.Console.Cli.Model;

/// <summary>
/// Represents an option of a command-line command.
/// </summary>
public sealed class CommandOption : CommandParameter, ICommandOptionAttributeData
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandOption"/>.
    /// </summary>
    /// <param name="name">The option name, including the option leader (prefix).</param>
    /// <param name="setter">A delegate that parses and assigns the value of the argument.</param>
    /// <param name="valueType">The type of value the option accepts. (If the option accepts multiple values, this is the type of each individually.)</param>
    /// <param name="description">A description of this parameter, which is used in generated help.</param>
    public CommandOption(
        string name,
        CommandParameterSetter setter,
        Type valueType,
        string? description)
        : base(name, setter, valueType, description)
    {
        this.Aliases = ImmutableArray<string>.Empty;
    }

    /// <inheritdoc/>
    public ImmutableArray<string> Aliases { get; init; }

    /// <summary>
    /// Indicates whether this option is a flag with an implicit value of <see langword="true"/> or <see langword="false"/>.
    /// </summary>
    public bool IsFlag { get; init; }

    /// <inheritdoc/>
    public bool IsHidden { get; init; }

    /// <summary>
    /// Enumerates the names of this option, including the primary name and any aliases.
    /// </summary>
    public IEnumerable<string> GetNames()
    {
        yield return this.Name;

        foreach (var alias in this.Aliases)
            yield return alias;
    }
}
