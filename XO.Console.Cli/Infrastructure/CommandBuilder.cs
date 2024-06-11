using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;

namespace XO.Console.Cli.Infrastructure;

/// <summary>
/// Configures a command.
/// </summary>
public sealed class CommandBuilder : ICommandBuilder
{
    private readonly string _verb;
    private readonly Func<ITypeResolver, ICommand?> _factory;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)]
    private readonly Type _parametersType;

    private ImmutableHashSet<string> _aliases;
    private ImmutableList<ConfiguredCommand> _commands;
    private string? _description;
    private bool _hidden;

    /// <summary>
    /// Initializes a new instance of <see cref="CommandBuilder"/>.
    /// </summary>
    /// <param name="verb">The verb that invokes this command.</param>
    /// <param name="factory">A factory delegate that creates instance(s) of the command implementation type.</param>
    /// <param name="parametersType">The type of the command's parameters.</param>
    public CommandBuilder(
        string verb,
        Func<ITypeResolver, ICommand?> factory,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type parametersType)
    {
        _verb = verb;
        _factory = factory;
        _parametersType = parametersType;

        _aliases = ImmutableHashSet<string>.Empty;
        _commands = ImmutableList<ConfiguredCommand>.Empty;
    }

    /// <summary>
    /// Finalizes the command configuration.
    /// </summary>
    /// <returns>A new instance of <see cref="ConfiguredCommand"/>.</returns>
    public ConfiguredCommand Build()
    {
        foreach (var command in _commands)
        {
            if (!_parametersType.IsAssignableFrom(command.ParametersType))
            {
                throw new CommandTypeException(
                    command.ParametersType,
                    $"Must be derived from the parent command's parameters type ({_parametersType})");
            }
        }

        return new ConfiguredCommand(_verb, _factory, _parametersType)
        {
            Aliases = _aliases,
            Commands = _commands,
            Description = _description,
            IsHidden = _hidden,
        };
    }

    /// <inheritdoc/>
    public ICommandBuilder AddAlias(string alias)
    {
        if (_verb != alias) _aliases = _aliases.Add(alias);
        return this;
    }

    /// <inheritdoc/>
    public ICommandBuilder AddBranch(
        string name,
        Action<ICommandBuilder> configure)
    {
        var builder = CreateMissing(name, _parametersType);

        configure(builder);
        return AddCommand(builder.Build());
    }

    /// <inheritdoc/>
    public ICommandBuilder AddBranch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string name,
        Action<ICommandBuilder> configure)
        where TParameters : CommandParameters
    {
        var builder = CreateMissing(name, typeof(TParameters));

        configure(builder);
        return AddCommand(builder.Build());
    }

    /// <inheritdoc/>
    public ICommandBuilder AddCommand<TCommand>(
        string verb,
        Action<ICommandBuilder>? configure = null)
        where TCommand : class, ICommand
    {
        var builder = TypeRegistry.CreateCommandBuilder<TCommand>(verb);

        configure?.Invoke(builder);
        return AddCommand(builder.Build());
    }

    /// <summary>
    /// Adds a pre-configured command.
    /// </summary>
    /// <param name="configuredCommand">The configured command.</param>
    public ICommandBuilder AddCommand(ConfiguredCommand configuredCommand)
    {
        ArgumentNullException.ThrowIfNull(configuredCommand);

        _commands = _commands.Add(configuredCommand);
        return this;
    }

    /// <inheritdoc/>
    public ICommandBuilder AddDelegate(
        string verb,
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
    {
        var builder = CreateDelegate(verb, executeAsync, _parametersType);

        configure?.Invoke(builder);
        return AddCommand(builder.Build());
    }

    /// <inheritdoc/>
    public ICommandBuilder AddDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string verb,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
        where TParameters : CommandParameters
    {
        var builder = CreateDelegate(verb, executeAsync);

        configure?.Invoke(builder);
        return AddCommand(builder.Build());
    }

    /// <inheritdoc/>
    public ICommandBuilder SetDescription(string description)
    {
        _description = description;
        return this;
    }

    /// <inheritdoc/>
    public ICommandBuilder SetHidden(bool hidden)
    {
        _hidden = hidden;
        return this;
    }

    internal static CommandBuilder CreateDelegate(
        string verb,
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type parametersType)
    {
        return new CommandBuilder(
            verb,
            (_) => new DelegateCommand<CommandParameters>(executeAsync),
            parametersType);
    }

    internal static CommandBuilder CreateDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string verb,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
    {
        return new CommandBuilder(
            verb,
            (_) => new DelegateCommand<TParameters>(executeAsync),
            typeof(TParameters));
    }

    internal static CommandBuilder CreateMissing(
        string verb,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] Type parametersType)
    {
        return new CommandBuilder(
            verb,
            static (_) => new MissingCommand(),
            parametersType);
    }
}
