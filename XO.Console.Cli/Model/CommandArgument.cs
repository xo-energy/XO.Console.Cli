namespace XO.Console.Cli.Model;

/// <summary>
/// Represents an argument to a command-line command.
/// </summary>
public sealed class CommandArgument : AbstractCommandParameter, ICommandArgumentAttributeData
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandArgument"/>.
    /// </summary>
    /// <param name="declaringType">The type that declares the target property.</param>
    /// <param name="propertyName">The name of the target property.</param>
    /// <param name="name">The argument name.</param>
    /// <param name="setter">A delegate that parses and assigns the value of the argument.</param>
    /// <param name="valueType">The type of value the argument accepts. (If the argument accepts multiple values, this is the type of each individually.)</param>
    /// <param name="description">A description of the argument, which is used in generated help.</param>
    public CommandArgument(
        Type declaringType,
        string propertyName,
        string name,
        CommandParameterSetter setter,
        Type valueType,
        string? description)
        : base(declaringType, propertyName, name, setter, valueType, description) { }

    /// <inheritdoc/>
    public int Order { get; init; }

    /// <inheritdoc/>
    public bool IsGreedy { get; init; }

    /// <inheritdoc/>
    public bool IsOptional { get; init; }
}
