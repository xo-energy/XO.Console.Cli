namespace XO.Console.Cli.Model;

/// <summary>
/// Defines the properties that can by configured by <see cref="CommandArgumentAttribute"/>.
/// </summary>
public interface ICommandArgumentAttributeData
{
    /// <summary>
    /// The order of this argument relative to the command's other arguments.
    /// </summary>
    public int Order { get; }

    /// <summary>
    /// The argument name.
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Describes the argument in generated help.
    /// </summary>
    public string? Description { get; }

    /// <summary>
    /// Indicates whether the argument consumes all remaining argument values.
    /// </summary>
    /// <remarks>
    /// A greedy argument must be the last argument to its command.
    /// </remarks>
    public bool IsGreedy { get; }

    /// <summary>
    /// Indicates whether the argument is optional.
    /// </summary>
    public bool IsOptional { get; }
}
