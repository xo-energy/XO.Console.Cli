using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace XO.Console.Cli;

/// <summary>
/// Represents an object that provides an instance of <see cref="ICommandBuilder"/>.
/// </summary>
/// <typeparam name="TProvider">The implementing provider type.</typeparam>
public interface ICommandBuilderProvider<TProvider>
    where TProvider : class
{
    /// <summary>
    /// Gets the provided <see cref="ICommandBuilder"/>.
    /// </summary>
    internal ICommandBuilder Builder { get; }

    /// <summary>
    /// Gets the implementing object for use as a return value (for fluent method call chaining).
    /// </summary>
    internal TProvider Self { get; }

    /// <summary>
    /// Adds a branch (a command that does nothing, but may have sub-commands).
    /// </summary>
    /// <param name="name">The verb that selects the branch.</param>
    /// <param name="configure">A delegate that configures the new branch.</param>
    public TProvider AddBranch(
        string name,
        Action<ICommandBuilder> configure)
    {
        Builder.AddCommand(name, Builder.ParametersType, configure: configure);
        return Self;
    }

    /// <inheritdoc cref="AddBranch"/>
    /// <remarks>
    /// <para>
    /// <typeparamref name="TParameters"/> must derive from this command's parameters type.
    /// </para>
    /// </remarks>
    /// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
    public TProvider AddBranch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters>(
        string name,
        Action<ICommandBuilder> configure)
        where TParameters : CommandParameters
    {
        Builder.AddCommand(name, typeof(TParameters), configure: configure);
        return Self;
    }

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
    public TProvider AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommand>(
        Action<ICommandBuilder>? configure = null)
        where TCommand : class, ICommand
    {
        if (typeof(TCommand).GetCustomAttribute<CommandAttribute>() is not CommandAttribute attr)
        {
            throw new ArgumentException(
                $"Type '{typeof(TCommand)}' must be decorated with {nameof(CommandAttribute)} (or, call '{nameof(AddCommand)}<{nameof(TCommand)}>(string verb)' instead)");
        }

        return AddCommand<TCommand>(attr.Verb, configure);
    }

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
    public TProvider AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommand>(
        string verb,
        Action<ICommandBuilder>? configure = null)
        where TCommand : class, ICommand
    {
        var parametersType = CommandBuilder.GetParametersType<TCommand>();

        // add the command to this command's children
        Builder.AddCommand(
            verb,
            parametersType,
            CommandBuilder.CreateCommandFactory<TCommand>(),
            builder =>
            {
                // initialize the command's description from the attribute, if present
                if (typeof(TCommand).GetCustomAttribute<DescriptionAttribute>() is DescriptionAttribute
                    descriptionAttribute)
                    builder.SetDescription(descriptionAttribute.Description);

                // call the user's configuration delegate
                configure?.Invoke(builder);
            });

        return Self;
    }

    /// <summary>
    /// Adds a command with a delegate implementation.
    /// </summary>
    /// <param name="verb">The verb that invokes the command.</param>
    /// <param name="executeAsync">The command implementation delegate.</param>
    /// <param name="configure">A delegate that configures the command.</param>
    public TProvider AddDelegate(
        string verb,
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
    {
        Builder.AddCommand(
            verb,
            Builder.ParametersType,
            CommandBuilder.CreateCommandFactory<CommandParameters>(
                (context, _, cancellationToken) => executeAsync(context, cancellationToken)),
            configure);

        return Self;
    }

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
    public TProvider AddDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters>(
        string verb,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
        where TParameters : CommandParameters
    {
        Builder.AddCommand(
            verb,
            typeof(TParameters),
            CommandBuilder.CreateCommandFactory(executeAsync),
            configure);

        return Self;
    }
}
