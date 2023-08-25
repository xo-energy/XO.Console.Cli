using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// Configures a command.
/// </summary>
/// <typeparam name="TCommand">The command implementation type.</typeparam>
/// <typeparam name="TParameters">The command parameters type.</typeparam>
public sealed class CommandBuilder<
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TCommand,
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>
    : CommandBuilder
    where TCommand : ICommand<TParameters>
    where TParameters : CommandParameters
{
    private readonly ImmutableHashSet<string>.Builder _aliases;
    private readonly ImmutableList<ConfiguredCommand>.Builder _commands;
    private readonly Func<ITypeResolver, TCommand?> _commandFactory;
    private readonly string _verb;

    private string? _description;
    private bool _hidden;

    /// <summary>
    /// Initializes a new instance of <see cref="CommandBuilder{TCommand, TParameters}"/>.
    /// </summary>
    /// <param name="verb">The verb that invokes this command.</param>
    /// <param name="factory">A factory delegate that creates instance(s) of the command implementation type.</param>
    public CommandBuilder(string verb, Func<ITypeResolver, TCommand?> factory)
    {
        _aliases = ImmutableHashSet.CreateBuilder<string>();
        _commands = ImmutableList.CreateBuilder<ConfiguredCommand>();
        _commandFactory = factory;
        _verb = verb;
    }

    /// <inheritdoc/>
    public override ConfiguredCommand Build()
    {
        var configuredCommands = _commands.ToImmutable();

        foreach (var command in configuredCommands)
        {
            if (!typeof(TParameters).IsAssignableFrom(command.ParametersType))
            {
                throw new CommandTypeException(
                    command.ParametersType,
                    $"Must be derived from the parent command's parameters type ({typeof(TParameters)})");
            }
        }

        return new ConfiguredCommand<TCommand, TParameters>(_verb, _commandFactory)
        {
            Aliases = _aliases.ToImmutable(),
            Commands = configuredCommands,
            Description = _description,
            IsHidden = _hidden,
        };
    }

    /// <inheritdoc/>
    public override ICommandBuilder AddAlias(string alias)
    {
        if (_verb != alias) _aliases.Add(alias);
        return this;
    }

    /// <inheritdoc/>
    public override ICommandBuilder AddBranch(
        string name,
        Action<ICommandBuilder> configure)
    {
        var builder = CreateMissing<TParameters>(name);
        return AddCommand(builder, configure);
    }

    /// <inheritdoc/>
    public override ICommandBuilder AddCommand(
        CommandBuilder builder,
        Action<ICommandBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        configure?.Invoke(builder);

        var configuredCommand = builder.Build();

        _commands.Add(configuredCommand);
        return this;
    }

    /// <inheritdoc/>
    public override ICommandBuilder AddDelegate(
        string verb,
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
    {
        var builder = FromDelegate<TParameters>(verb, executeAsync);
        return AddCommand(builder, configure);
    }

    /// <inheritdoc/>
    public override ICommandBuilder SetDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <inheritdoc/>
    public override ICommandBuilder SetHidden(bool hidden)
    {
        _hidden = hidden;
        return this;
    }
}
