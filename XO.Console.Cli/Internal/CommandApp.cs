using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using XO.Console.Cli.Commands;

namespace XO.Console.Cli;

internal sealed class CommandApp : ICommandApp
{
    public const string ExplicitArgumentsOption = "--";

    private readonly ITypeResolver _resolver;
    private readonly ExecutorDelegate _pipeline;
    private readonly ConfiguredCommand _rootCommand;
    private readonly CommandAppSettings _settings;

    private readonly Builtins.Options _builtinOptions;
    private readonly CommandParametersBinder _binder;
    private readonly CommandParametersInspector _inspector;

    public CommandApp(
        ITypeResolver resolver,
        ExecutorDelegate pipeline,
        ConfiguredCommand rootCommand,
        CommandAppSettings settings)
    {
        _resolver = resolver;
        _pipeline = pipeline;
        _rootCommand = rootCommand;
        _settings = settings;

        _builtinOptions = new Builtins.Options(settings.OptionStyle);
        _binder = new CommandParametersBinder(settings.Converters);
        _inspector = new CommandParametersInspector(settings.OptionStyle, settings.OptionNameComparer);
    }

    public ConfiguredCommand RootCommand
        => _rootCommand;

    public CommandAppSettings Settings
        => _settings;

    public Builtins.Options BuiltinOptions
        => _builtinOptions;

    /// <inheritdoc/>
    public CommandParseResult Parse(IReadOnlyList<string> args)
    {
        ArgumentNullException.ThrowIfNull(args);

        var state = new CommandParserState(args.Count, _rootCommand.Commands, _settings);

        state.AddOptions(_builtinOptions);
        state.AddOptions(_settings.GlobalOptions);
        state.AddParameters(_inspector.InspectParameters(_rootCommand));

        // parse args
        for (int i = 0; i < args.Count; ++i)
        {
            if (!state.ExplicitArguments
                && args[i].StartsWith(_settings.OptionLeader)
                && state.TryGetOption(
                    args[i].Split(_settings.OptionValueSeparator, 2),
                    out var option,
                    out var value))
            {
                // check we have a value and it doesn't look like another option
                if (value != null)
                {
                    state.Tokens[i] = new CommandToken(CommandTokenType.OptionValue, value, option);
                }
                else if (option.IsFlag)
                {
                    state.Tokens[i] = new CommandToken(CommandTokenType.Option, args[i], option);
                }
                else if (i + 1 < args.Count && !MustParseAsOptionName(args[i + 1]))
                {
                    state.Tokens[i] = new CommandToken(CommandTokenType.Option, args[i], option);
                    i++;
                    state.Tokens[i] = new CommandToken(CommandTokenType.OptionValue, args[i], option);
                }
                else
                {
                    state.Tokens[i] = new CommandToken(CommandTokenType.Option, args[i], option);
                    state.Errors.Add($"Expected a value for {args[i]}");
                }
            }
            else if (!state.ExplicitArguments
                && args[i].StartsWith(_settings.OptionLeader)
                && state.TryGetShortOptionGroup(args[i], out var optionGroup))
            {
                state.Tokens[i] = new CommandToken(CommandTokenType.OptionGroup, args[i], optionGroup);
            }
            else if (!state.ExplicitArguments && args[i] == ExplicitArgumentsOption)
            {
                state.ExplicitArguments = true;
                state.Tokens[i] = new CommandToken(
                    CommandTokenType.System,
                    args[i],
                    "Explicit arguments enabled");
            }
            else if (!state.ExplicitArguments && MustParseAsOptionName(args[i]))
            {
                state.Tokens[i] = new CommandToken(
                    CommandTokenType.Unknown,
                    args[i],
                    "Unexpected option");
            }
            else if (state.Arguments.TryPeek(out var argument))
            {
                state.Tokens[i] = new CommandToken(CommandTokenType.Argument, args[i], argument);

                if (!argument.Attribute.IsGreedy)
                    _ = state.Arguments.Dequeue();
            }
            else if (!state.ExplicitArguments
                && state.Commands.FirstOrDefault(x => x.IsMatch(args[i])) is ConfiguredCommand next)
            {
                state.Tokens[i] = new CommandToken(CommandTokenType.Command, args[i], next);

                state.Commands = next.Commands;

                // do not accept the '--version' builtin option for subcommands
                foreach (var name in _builtinOptions.Version.GetNames())
                    state.Options.Remove(name);

                state.AddParameters(_inspector.InspectParameters(next));
            }
            else
            {
                state.Tokens[i] = new CommandToken(
                    CommandTokenType.Unknown,
                    args[i],
                    "Unexpected argument");
            }
        }

        // check for missing arguments
        foreach (var argument in state.Arguments)
        {
            if (argument.Attribute.IsOptional)
                continue;
            if (argument.Attribute.IsGreedy && state.Tokens.Any(x => argument.Equals(x.Context)))
                continue;

            state.Errors.Add($"Missing required argument '{argument.Attribute.Name}'");
        }

        return new CommandParseResult(
            state.Tokens.ToImmutableArray(),
            state.Errors.ToImmutable());
    }

    /// <inheritdoc/>
    public CommandContext Bind(CommandParseResult parseResult)
    {
        ArgumentNullException.ThrowIfNull(parseResult);

        var command = _rootCommand;
        var hasErrors = parseResult.Errors.Count > 0;

        // scan tokens
        foreach (var token in parseResult.Tokens)
        {
            switch (token.TokenType)
            {
                case CommandTokenType.Command:
                    command = (ConfiguredCommand)token.Context!;
                    break;

                case CommandTokenType.Option when Object.ReferenceEquals(token.Context, _builtinOptions.CliExplain):
                    return BindInternal(
                        parseResult,
                        _ => new CliExplainCommand(),
                        typeof(CommandParameters),
                        ImmutableDictionary<CommandParameter, object?>.Empty);

                case CommandTokenType.Option when Object.ReferenceEquals(token.Context, _builtinOptions.Help):
                    return BindInternal(
                        parseResult,
                        _ => new HelpCommand(this, _inspector),
                        typeof(CommandParameters),
                        ImmutableDictionary<CommandParameter, object?>.Empty);

                case CommandTokenType.Option when Object.ReferenceEquals(token.Context, _builtinOptions.Version):
                    return BindInternal(
                        parseResult,
                        _ => new VersionCommand(_settings.ApplicationVersion),
                        typeof(CommandParameters),
                        ImmutableDictionary<CommandParameter, object?>.Empty);

                case CommandTokenType.Unknown:
                    hasErrors = true;
                    break;
            }
        }

        // surface errors from parsing
        if (_settings.Strict && hasErrors)
            throw new CommandParsingException(parseResult);

        // bind parameter values
        var bindings = _binder.BindParameters(parseResult.Tokens);

        // create command and bind parameters instance
        return BindInternal(parseResult, command.CommandFactory, command.ParametersType, bindings);
    }

    private CommandContext BindInternal(
        CommandParseResult parseResult,
        CommandFactory commandFactory,
        [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type parametersType,
        ImmutableDictionary<CommandParameter, object?> bindings)
    {
        ITypeResolverScope scope;

        // create a service scope, if our resolver supports it
        if (_resolver is ITypeResolverScopeFactory scopeFactory)
        {
            scope = scopeFactory.CreateScope();
        }
        else
        {
            scope = new DefaultTypeResolverScope(_resolver);
        }

        try
        {
            // create an instance of the command type
            if (commandFactory(scope.TypeResolver) is not ICommand commandInstance)
                throw new CommandTypeException(typeof(CommandFactory), $"Failed to instantiate command!");

            // create parameters instance
            if (scope.TypeResolver.Get(parametersType) is not CommandParameters parameters)
                throw new CommandTypeException(parametersType, "Failed to instantiate parameters");

            // create context
            var context = new CommandContext(scope, commandInstance, parameters, parseResult);

            // bind parameters
            foreach (var (parameter, value) in bindings)
                parameter.Setter(context, value);

            // validate parameters
            var validationResult = parameters.Validate();
            if (validationResult != ValidationResult.Success)
                throw new CommandParameterValidationException(validationResult);

            return context;
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        // go!
        return await _pipeline(context, cancellationToken).ConfigureAwait(false);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private bool MustParseAsOptionName(string arg)
    {
        // allow a solo '-' to mean stdin or stdout
        if (arg == "-")
            return false;

        if (arg.StartsWith(_settings.OptionLeader))
            return _settings.OptionLeaderMustStartOption;

        return false;
    }
}
