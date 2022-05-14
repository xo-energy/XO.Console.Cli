namespace XO.Console.Cli;

/// <summary>
/// Represents an argument or option to a command-line command.
/// </summary>
public abstract class CommandParameter : IEquatable<CommandParameter>
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandParameter"/>.
    /// </summary>
    /// <param name="declaringType">The type that declares this parameter.</param>
    /// <param name="valueType">The type of the parameter's value.</param>
    /// <param name="setter">A delegate that sets the value of this parameter.</param>
    /// <param name="description">A description of this parameter, which is used in generated help.</param>
    protected CommandParameter(
        Type declaringType,
        Type valueType,
        Action<CommandContext, object?> setter,
        string? description = null)
    {
        this.DeclaringType = declaringType;
        this.Setter = setter;
        this.ValueType = valueType;
        this.Description = description;
    }

    /// <summary>
    /// Gets the parameter name.
    /// </summary>
    public abstract string Name { get; }

    /// <summary>
    /// Gets the type that declares this parameter (usually a subclass of <see cref="CommandParameters"/>).
    /// </summary>
    public Type DeclaringType { get; }

    /// <summary>
    /// Gets the type of this parameter's value.
    /// </summary>
    public Type ValueType { get; }

    /// <summary>
    /// Gets a delegate that sets the value of this parameter.
    /// </summary>
    /// <remarks>
    /// Usually, this is a reference to a property setter on <see cref="DeclaringType"/>.
    /// </remarks>
    public Action<CommandContext, object?> Setter { get; }

    /// <summary>
    /// Gets a description of this parameter, used in generated help.
    /// </summary>
    public string? Description { get; }

    /// <inheritdoc/>
    public bool Equals(CommandParameter? other)
    {
        return other != null
            && this.DeclaringType == other.DeclaringType
            && this.Name == other.Name;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
        => Equals(obj as CommandParameter);

    /// <inheritdoc/>
    public override int GetHashCode()
        => HashCode.Combine(this.DeclaringType, this.Name);

    /// <inheritdoc/>
    public override string ToString()
        => $"{this.ValueType} '{this.Name}' ({this.DeclaringType})";

    /// <summary>
    /// Indicates whether <see cref="CommandParameter"/> instances are equal.
    /// </summary>
    public static bool operator ==(CommandParameter? x, CommandParameter? y)
        => Object.Equals(x, y);

    /// <summary>
    /// Indicates whether <see cref="CommandParameter"/> instances are not equal.
    /// </summary>
    public static bool operator !=(CommandParameter? x, CommandParameter? y)
        => !Object.Equals(x, y);
}
