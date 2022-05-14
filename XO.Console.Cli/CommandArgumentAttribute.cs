namespace XO.Console.Cli;

/// <summary>
/// Configures a property as a command-line argument.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class CommandArgumentAttribute : Attribute
{
    /// <summary>
    /// Creates a new instance of <see cref="CommandArgumentAttribute"/>.
    /// </summary>
    /// <param name="order">The order of this argument relative to the command's other arguments.</param>
    /// <param name="name">The argument name, used in generated help.</param>
    public CommandArgumentAttribute(int order, string name)
    {
        this.Order = order;
        this.Name = name;
    }

    /// <summary>
    /// Gets the order of this argument relative to the command's other arguments.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// Gets the argument name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Gets or sets whether the argument consumes all remaining argument values.
    /// </summary>
    /// <remarks>
    /// A greedy argument must be the last argument to its command.
    /// </remarks>
    public bool IsGreedy { get; set; }

    /// <summary>
    /// Gets or sets whether the argument is optional.
    /// </summary>
    public bool IsOptional { get; set; }
}
