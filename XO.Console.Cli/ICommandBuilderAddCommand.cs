using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli;

/// <summary>
/// Declares methods that add child commands to a builder of type <typeparamref name="TSelf"/>.
/// </summary>
public interface ICommandBuilderAddCommand<TSelf>
    where TSelf : class
{
    /// <summary>
    /// Adds a branch (a command that does nothing, but may have sub-commands).
    /// </summary>
    /// <param name="name">The verb that selects the branch.</param>
    /// <param name="configure">A delegate that configures the new branch.</param>
    TSelf AddBranch(string name, Action<ICommandBuilder> configure);

    /// <inheritdoc cref="AddBranch"/>
    /// <remarks>
    /// <para>
    /// <typeparamref name="TParameters"/> must derive from this command's parameters type.
    /// </para>
    /// </remarks>
    /// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
    TSelf AddBranch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters>(string name, Action<ICommandBuilder> configure)
        where TParameters : CommandParameters;

    /// <summary>
    /// Adds a command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The command implementation <typeparamref name="TCommand"/> must be decorated with
    /// <see cref="CommandAttribute"/> to specify the verb that invokes the command, and it must accept parameters of a
    /// type derived from this command's parameters type.
    /// </para>
    /// <para>
    /// The command's description is initialized from any instance of <see cref="DescriptionAttribute"/> decorating
    /// <typeparamref name="TCommand"/>.
    /// </para>
    /// </remarks>
    /// <param name="configure">A delegate that configures the command.</param>
    /// <typeparam name="TCommand">The command implementation type.</typeparam>
    TSelf AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommand>(Action<ICommandBuilder>? configure = null)
        where TCommand : class, ICommand;

    /// <summary>
    /// Adds a command.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The command implementation <typeparamref name="TCommand"/> must accept parameters of a type derived from
    /// this command's parameters type.
    /// </para>
    /// <para>
    /// The command's description is initialized from any instance of <see cref="DescriptionAttribute"/> decorating
    /// <typeparamref name="TCommand"/>.
    /// </para>
    /// </remarks>
    /// <param name="verb">The verb that invokes the command.</param>
    /// <param name="configure">A delegate that configures the command.</param>
    /// <typeparam name="TCommand">The command implementation type.</typeparam>
    TSelf AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommand>(string verb, Action<ICommandBuilder>? configure = null)
        where TCommand : class, ICommand;

    /// <summary>
    /// Adds a command with a delegate implementation.
    /// </summary>
    /// <param name="verb">The verb that invokes the command.</param>
    /// <param name="executeAsync">The command implementation delegate.</param>
    /// <param name="configure">A delegate that configures the command.</param>
    TSelf AddDelegate(string verb, Func<ICommandContext, CancellationToken, Task<int>> executeAsync, Action<ICommandBuilder>? configure = null);

    /// <summary>
    /// Adds a command with a delegate implementation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <typeparamref name="TParameters"/> must derive from this command's parameters type.
    /// </para>
    /// </remarks>
    /// <param name="verb">The verb that invokes the command.</param>
    /// <param name="executeAsync">The command implementation delegate.</param>
    /// <param name="configure">A delegate that configures the command.</param>
    /// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
    TSelf AddDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters>(
        string verb,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
        where TParameters : CommandParameters;
}
