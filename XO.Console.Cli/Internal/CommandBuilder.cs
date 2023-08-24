using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using XO.Console.Cli.Commands;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

internal class CommandBuilder : ICommandBuilder
{
    private static readonly CommandFactory MissingCommandFactory
        = _ => new MissingCommand();

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private readonly Type _parametersType;
    private readonly string _verb;
    private readonly ImmutableHashSet<string>.Builder _aliases;
    private readonly ImmutableList<ConfiguredCommand>.Builder _commands;

    private CommandFactory _commandFactory;
    private string? _description;
    private bool _hidden;

    public CommandBuilder(
        string verb,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type parametersType,
        CommandFactory? factory = null)
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

    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ParametersType
        => _parametersType;

    public ICommandBuilder AddAlias(string alias)
    {
        if (_verb != alias) _aliases.Add(alias);
        return this;
    }

    public ICommandBuilder AddCommand(
        string verb,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type parametersType,
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

    public static CommandFactory CreateCommandFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommand>()
        where TCommand : class, ICommand
    {
        return (resolver) => resolver.Get<TCommand>();
    }

    public static CommandFactory CreateCommandFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters>(
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
    {
        return _ => new DelegateCommand<TParameters>(executeAsync);
    }

    [return: DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    [SuppressMessage(
        "Trimming",
        "IL2063:The return value of method has a DynamicallyAccessedMembersAttribute, but the value returned from the method can not be statically analyzed.",
        Justification = "The generic type argument TParameters in ICommand<TParameters> has a matching DynamicallyAccessedMembersAttribute")]
    public static Type GetParametersType<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.Interfaces)] TCommand>()
        where TCommand : class, ICommand
    {
        // look for an implementation of ICommand<TParameters> (all commands must have one)
        var @interface = typeof(TCommand)
            .GetInterfaces()
            .Single(x => x.IsGenericType && x.GetGenericTypeDefinition() == typeof(ICommand<>));

        return @interface.GenericTypeArguments[0];
    }
}
