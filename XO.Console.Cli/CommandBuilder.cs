using System.Collections.Immutable;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XO.Console.Cli.Commands;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// Configures a command.
/// </summary>
public sealed class CommandBuilder : ICommandBuilder
{
    private static readonly CommandFactory MissingCommandFactory
        = _ => new MissingCommand();

    private readonly string _verb;
    private readonly ImmutableHashSet<string>.Builder _aliases;
    private readonly ImmutableList<ConfiguredCommand>.Builder _commands;

    private CommandFactory _commandFactory;
    private string? _description;
    private bool _hidden;
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    private Type _parametersType;

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

    internal ConfiguredCommand Build()
    {
        var configuredCommands = _commands.ToImmutable();

        foreach (var command in configuredCommands)
        {
            if (!_parametersType.IsAssignableFrom(command.ParametersType))
            {
                throw new CommandTypeException(
                    command.ParametersType,
                    $"Must be derived from the parent command's parameters type ({_parametersType})");
            }
        }

        return new ConfiguredCommand(_commandFactory, _parametersType, _verb)
        {
            Aliases = _aliases.ToImmutable(),
            Commands = configuredCommands,
            Description = _description,
            IsHidden = _hidden,
        };
    }

    /// <summary>
    /// Gets the type that describes the command's parameters.
    /// </summary>
    [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)]
    public Type ParametersType
        => _parametersType;

    public ICommandBuilder AddAlias(string alias)
    {
        if (_verb != alias) _aliases.Add(alias);
        return this;
    }

    /// <inheritdoc/>
    public ICommandBuilder AddBranch(
        string name,
        Action<ICommandBuilder> configure)
    {
        AddCommand(name, ParametersType, configure: configure);
        return this;
    }

    /// <inheritdoc/>
    public ICommandBuilder AddBranch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters>(
        string name,
        Action<ICommandBuilder> configure)
        where TParameters : CommandParameters
    {
        AddCommand(name, typeof(TParameters), configure: configure);
        return this;
    }

    /// <inheritdoc/>
    public ICommandBuilder AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommand>(
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

    /// <inheritdoc/>
    public ICommandBuilder AddCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommand>(
        string verb,
        Action<ICommandBuilder>? configure = null)
        where TCommand : class, ICommand
    {
        var parametersType = CommandBuilder.GetParametersType<TCommand>();

        // add the command to this command's children
        AddCommand(
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

        return this;
    }

    /// <inheritdoc/>
    public ICommandBuilder AddDelegate(
        string verb,
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
    {
        AddCommand(
            verb,
            ParametersType,
            CommandBuilder.CreateCommandFactory<CommandParameters>(
                (context, _, cancellationToken) => executeAsync(context, cancellationToken)),
            configure);

        return this;
    }

    /// <inheritdoc/>
    public ICommandBuilder AddDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters>(
        string verb,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
        where TParameters : CommandParameters
    {
        AddCommand(
            verb,
            typeof(TParameters),
            CommandBuilder.CreateCommandFactory(executeAsync),
            configure);

        return this;
    }

    public ICommandBuilder SetCommandFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TCommand>()
        where TCommand : class, ICommand
    {
        _commandFactory = CreateCommandFactory<TCommand>();
        _parametersType = GetParametersType<TCommand>();
        return this;
    }

    public ICommandBuilder SetCommandFactory<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] TParameters>(
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
    {
        _commandFactory = CreateCommandFactory(executeAsync);
        _parametersType = typeof(TParameters);
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
    /// <returns>The <see cref="CommandBuilder"/>.</returns>
    private CommandBuilder AddCommand(
        string verb,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type parametersType,
        CommandFactory? commandFactory = null,
        Action<ICommandBuilder>? configure = null)
    {
        ArgumentNullException.ThrowIfNull(verb);
        ArgumentNullException.ThrowIfNull(parametersType);

        var builder = new CommandBuilder(verb, parametersType, commandFactory);

        configure?.Invoke(builder);

        _commands.Add(builder.Build());
        return this;
    }

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
