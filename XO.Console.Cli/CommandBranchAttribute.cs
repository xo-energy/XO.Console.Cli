namespace XO.Console.Cli;

/// <summary>
/// Configures a custom subclass of <see cref="CommandAttribute"/> for declarative configuration of sub-commands.
/// </summary>
/// <remarks>
/// Apply this attribute to a custom subclass of <see cref="CommandAttribute"/>. Then, apply the new custom attribute
/// instead of <see cref="CommandAttribute"/> to create a sub-command with the specified path.
/// </remarks>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public sealed class CommandBranchAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of <see cref="CommandBranchAttribute"/>.
    /// </summary>
    /// <param name="path">The sequence of &quot;parent&quot; verbs for commands configured by the target attribute.</param>
    public CommandBranchAttribute(params string[] path)
    {
        this.Path = path;
    }

    /// <summary>
    /// A collection of aliases that may be substituted for the last verb in the path.
    /// </summary>
    public string[] Aliases { get; set; } = Array.Empty<string>();

    /// <summary>
    /// Describes the branch in generated help.
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Sets whether the command is hidden from generated help.
    /// </summary>
    public bool IsHidden { get; set; }

    /// <summary>
    /// Sets the base parameters type for commands in this branch.
    /// </summary>
    public Type? ParametersType { get; set; }

    /// <summary>
    /// The branch path.
    /// </summary>
    public IReadOnlyList<string> Path { get; }
}
