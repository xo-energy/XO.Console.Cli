namespace XO.Console.Cli.Model;

/// <summary>
/// Represents a delegate that parses and assigns the value of a command parameter.
/// </summary>
/// <param name="context">The command execution context. This delegate is expected to set a value on <see cref="CommandContext.Parameters"/> or the context itself.</param>
/// <param name="values">The sequence of string values to convert.</param>
/// <param name="converters">A collection of runtime-configured converters.</param>
public delegate void CommandParameterSetter(
    CommandContext context,
    IEnumerable<string> values,
    IReadOnlyDictionary<Type, Delegate> converters);

/// <summary>
/// Represents an argument or option to a command-line command.
/// </summary>
public abstract class CommandParameter
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandParameter"/>.
    /// </summary>
    /// <param name="name">The parameter name.</param>
    /// <param name="setter">A delegate that parses and assigns the value of the argument.</param>
    /// <param name="valueType">The type of value the parameter accepts. (If the argument accepts multiple values, this is the type of each individually.)</param>
    /// <param name="description">A description of this parameter, which is used in generated help.</param>
    protected CommandParameter(
        string name,
        CommandParameterSetter setter,
        Type valueType,
        string? description)
    {
        this.Name = name;
        this.Setter = setter;
        this.ValueType = valueType;
        this.Description = description;
    }

    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets a delegate that sets the value of this parameter.
    /// </summary>
    public CommandParameterSetter Setter { get; }

    /// <summary>
    /// Get the type of value this parameter accepts.
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// Gets a description of this parameter, used in generated help.
    /// </summary>
    public string? Description { get; }

    /// <inheritdoc/>
    public override string ToString()
        => this.Name;
}
