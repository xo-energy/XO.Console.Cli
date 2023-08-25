using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli;

/// <summary>
/// Configures a command.
/// </summary>
public interface ICommandBuilder : ICommandBuilderAddCommand<ICommandBuilder>
{
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
