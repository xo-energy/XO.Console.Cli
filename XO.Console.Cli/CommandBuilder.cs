using System.Diagnostics.CodeAnalysis;
using XO.Console.Cli.Commands;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// Configures a command.
/// </summary>
public abstract class CommandBuilder : ICommandBuilder
{
    /// <summary>
    /// Finalizes the command configuration.
    /// </summary>
    /// <returns>A new instance of <see cref="ConfiguredCommand"/>.</returns>
    public abstract ConfiguredCommand Build();

    /// <inheritdoc/>
    public abstract ICommandBuilder AddAlias(string alias);

    /// <inheritdoc/>
    public abstract ICommandBuilder AddBranch(string name, Action<ICommandBuilder> configure);

    /// <inheritdoc/>
    public ICommandBuilder AddBranch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string name,
        Action<ICommandBuilder> configure)
        where TParameters : CommandParameters
    {
        var builder = CreateMissing<TParameters>(name);

        AddCommand(builder, configure);
        return this;
    }

    /// <inheritdoc/>
    public abstract ICommandBuilder AddCommand(CommandBuilder builder, Action<ICommandBuilder>? configure = null);

    /// <inheritdoc/>
    public ICommandBuilder AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand>(
        string verb,
        Action<ICommandBuilder>? configure = null)
        where TCommand : class, ICommand
    {
        var builder = TypeRegistry.CreateCommandBuilder<TCommand>(verb);

        // add the command to this command's children
        AddCommand(builder, configure);
        return this;
    }

    /// <inheritdoc/>
    public abstract ICommandBuilder AddDelegate(
        string verb,
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null);

    /// <inheritdoc/>
    public ICommandBuilder AddDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string verb,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
        where TParameters : CommandParameters
    {
        var builder = new CommandBuilder<DelegateCommand<TParameters>, TParameters>(
            verb,
            _ => new DelegateCommand<TParameters>(executeAsync));

        AddCommand(builder, configure);
        return this;
    }

    /// <inheritdoc/>
    public abstract ICommandBuilder SetDescription(string description);

    /// <inheritdoc/>
    public abstract ICommandBuilder SetHidden(bool hidden);

    internal static CommandBuilder CreateMissing<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string verb)
        where TParameters : CommandParameters
    {
        var builder = new CommandBuilder<MissingCommand<TParameters>, TParameters>(
            verb,
            static (_) => new MissingCommand<TParameters>());

        return builder;
    }

    internal static CommandBuilder FromDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string verb,
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
    {
        var builder = new CommandBuilder<DelegateCommand<TParameters>, TParameters>(
            verb,
            _ => new DelegateCommand<TParameters>(executeAsync));

        return builder;
    }

    internal static CommandBuilder FromDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string verb,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
    {
        var builder = new CommandBuilder<DelegateCommand<TParameters>, TParameters>(
            verb,
            _ => new DelegateCommand<TParameters>(executeAsync));

        return builder;
    }
}
