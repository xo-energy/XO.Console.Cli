using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// Configures a property as a command-line argument.
/// </summary>
[AttributeUsage(AttributeTargets.Property, Inherited = false, AllowMultiple = false)]
public class CommandArgumentAttribute : Attribute, ICommandArgumentAttributeData
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

    /// <inheritdoc/>
    public int Order { get; }

    /// <inheritdoc/>
    public string Name { get; }

    /// <inheritdoc/>
    public bool IsGreedy { get; set; }

    /// <inheritdoc/>
    public bool IsOptional { get; set; }
}
