namespace XO.Console.Cli;

/// <summary>
/// Configures a command.
/// </summary>
public interface ICommandBuilder : ICommandBuilderProvider<ICommandBuilder>
{
    /// <summary>
    /// Gets the type that describes the command's parameters.
    /// </summary>
    Type ParametersType { get; }

    /// <summary>
    /// Adds an alias for this command.
    /// </summary>
    /// <remarks>
    /// Duplicate aliases are ignored.
    /// </remarks>
    /// <param name="alias">The alias for this command's verb.</param>
    /// <returns>The <see cref="ICommandBuilder"/>.</returns>
    ICommandBuilder AddAlias(string alias);

    /// <summary>
    /// Adds a command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <paramref name="parametersType"/> must derive from <see cref="ParametersType"/>.
    /// </para>
    /// </remarks>
    /// <param name="verb">The verb that invokes the command.</param>
    /// <param name="parametersType">The type whose properties describe the command's parameters.</param>
    /// <param name="commandFactory">A delegate that constructs a new instance of the command implementation.</param>
    /// <param name="configure">A delegate that configures the command.</param>
    /// <returns>The <see cref="ICommandBuilder"/>.</returns>
    ICommandBuilder AddCommand(
        string verb,
        Type parametersType,
        CommandFactory? commandFactory = null,
        Action<ICommandBuilder>? configure = null);

    /// <summary>
    /// Sets the command description, which is displayed in generated command help.
    /// </summary>
    /// <param name="description">The command description.</param>
    /// <returns>The <see cref="ICommandBuilder"/>.</returns>
    ICommandBuilder SetDescription(string description);

    /// <summary>
    /// Sets whether the command is hidden from generated command help.
    /// </summary>
    /// <param name="hidden">If <see langword="true"/>, the command is hidden from help output.</param>
    /// <returns>The <see cref="ICommandBuilder"/>.</returns>
    ICommandBuilder SetHidden(bool hidden);
}
