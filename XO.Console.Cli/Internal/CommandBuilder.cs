using System.Collections.Immutable;
using XO.Console.Cli.Commands;

namespace XO.Console.Cli;

internal class CommandBuilder : ICommandBuilder
{
    private static readonly CommandFactory MissingCommandFactory
        = _ => new MissingCommand();

    private readonly Type _parametersType;
    private readonly string _verb;
    private readonly ImmutableHashSet<string>.Builder _aliases;
    private readonly ImmutableList<ConfiguredCommand>.Builder _commands;

    private CommandFactory _commandFactory;
    private string? _description;
    private bool _hidden;

    public CommandBuilder(string verb, Type parametersType, CommandFactory? factory = null)
    {
        _commandFactory = factory ?? MissingCommandFactory;
        _parametersType = parametersType;
        _verb = verb;
        _aliases = ImmutableHashSet.CreateBuilder<string>();
        _commands = ImmutableList.CreateBuilder<ConfiguredCommand>();
    }

    public ConfiguredCommand Build()
    {
        return new ConfiguredCommand(_commandFactory, _parametersType, _verb)
        {
            Aliases = _aliases.ToImmutable(),
            Commands = _commands.ToImmutable(),
            Description = _description,
            IsHidden = _hidden,
        };
    }

    public void ThrowIfInvalidParametersType(Type parametersType)
    {
        if (!_parametersType.IsAssignableFrom(parametersType))
        {
            throw new CommandTypeException(
                parametersType,
                $"Must be derived from the parent command's parameters type ({_parametersType})");
        }
    }

    #region ICommandBuilder Implementation

    ICommandBuilder ICommandBuilderProvider<ICommandBuilder>.Builder
        => this;

    ICommandBuilder ICommandBuilderProvider<ICommandBuilder>.Self
        => this;

    public Type ParametersType
        => _parametersType;

    public ICommandBuilder AddAlias(string alias)
    {
        if (_verb != alias) _aliases.Add(alias);
        return this;
    }

    public ICommandBuilder AddCommand(
        string verb,
        Type parametersType,
        CommandFactory? commandFactory = null,
        Action<ICommandBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(verb);
        ArgumentNullException.ThrowIfNull(parametersType);
        ThrowIfInvalidParametersType(parametersType);

        var builder = new CommandBuilder(verb, parametersType, commandFactory);

        configure?.Invoke(builder);

        _commands.Add(builder.Build());
        return this;
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

    #endregion

    public static CommandFactory CreateCommandFactory<TCommand>()
        where TCommand : class, ICommand
    {
        return (resolver) => resolver.Get<TCommand>();
    }

    public static CommandFactory CreateCommandFactory<TParameters>(
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
    {
        return _ => new DelegateCommand<TParameters>(executeAsync);
    }

    public static Type GetParametersType<TCommand>()
        where TCommand : class, ICommand
    {
        // look for an implementation of ICommand<TParameters> (all commands must have one)
        var @interface = typeof(TCommand)
            .GetInterfaces()
            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommand<>));

        return @interface.GenericTypeArguments[0];
    }
}
