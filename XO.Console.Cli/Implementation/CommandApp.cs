using System.Collections.Immutable;
using System.ComponentModel.DataAnnotations;
using System.Runtime.CompilerServices;
using XO.Console.Cli.Commands;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Model;

namespace XO.Console.Cli.Implementation;

internal sealed class CommandApp : ICommandApp
{
    public const string ExplicitArgumentsOption = "--";

    private readonly ITypeResolver _resolver;
    private readonly ExecutorDelegate _pipeline;
    private readonly ConfiguredCommand _rootCommand;
    private readonly CommandAppSettings _settings;

    private readonly Builtins.Options _builtinOptions;

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

        foreach (var option in _builtinOptions)
            state.AddOption(typeof(Builtins.Options), option);
        foreach (var option in _settings.GlobalOptions)
            state.AddOption(typeof(CommandAppSettings), option);

        state.AddParameters(_rootCommand);

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

                if (!argument.IsGreedy)
                    _ = state.Arguments.Dequeue();
            }
            else if (!state.ExplicitArguments
                && state.Commands.FirstOrDefault(x => x.IsMatch(args[i])) is ConfiguredCommand next)
            {
                state.Tokens[i] = new CommandToken(CommandTokenType.Command, args[i], next);

                state.Commands = next.Commands;

                // do not accept the '--version' builtin option for subcommands
                state.Options.Remove(_builtinOptions.Version.Name);

                state.AddParameters(next);
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
            if (argument.IsOptional)
                continue;
            if (argument.IsGreedy && state.Tokens.Any(x => argument.Equals(x.Context)))
                continue;

            state.Errors.Add($"Missing required argument '{argument.Name}'");
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
                        new ConfiguredCommand(
                            _builtinOptions.CliExplain.Name,
                            static _ => new CliExplainCommand(),
                            typeof(CommandParameters)));

                case CommandTokenType.Option when Object.ReferenceEquals(token.Context, _builtinOptions.Help):
                    return BindInternal(
                        parseResult,
                        new ConfiguredCommand(
                            _builtinOptions.Help.Name,
                            _ => new HelpCommand(this),
                            typeof(CommandParameters)));

                case CommandTokenType.Option when Object.ReferenceEquals(token.Context, _builtinOptions.Version):
                    return BindInternal(
                        parseResult,
                        new ConfiguredCommand(
                            _builtinOptions.Version.Name,
                            _ => new VersionCommand(_settings.ApplicationVersion),
                            typeof(CommandParameters)));

                case CommandTokenType.Unknown:
                    hasErrors = true;
                    break;
            }
        }

        // surface errors from parsing
        if (_settings.Strict && hasErrors)
            throw new CommandParsingException(parseResult);

        // create command and bind parameters instance
        return BindInternal(parseResult, command);
    }

    private CommandContext BindInternal(CommandParseResult parseResult, ConfiguredCommand command)
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
            var commandInstance = command.CreateCommand(scope.TypeResolver);

            // create parameters instance
            var parameters = command.CreateParameters(scope.TypeResolver);

            // create context
            var context = new CommandContext(scope, commandInstance, parameters, parseResult);

            // bind parameters
            BindParameters(context, parseResult.Tokens);

            return context;
        }
        catch
        {
            scope.Dispose();
            throw;
        }
    }

    public void BindParameters(CommandContext context, IEnumerable<CommandToken> tokens)
    {
        // group tokens by their parameter (some may have multiple values)
        var parameterTokens = GetTokensByParameter(tokens)
            .GroupBy(x => x.Parameter, x => x.Value);

        // assign values by calling each parameter's setter
        foreach (var group in parameterTokens)
            group.Key.Setter(context, group, _settings.Converters);

        // validate
        var validationResult = context.Parameters.Validate();
        if (validationResult != ValidationResult.Success)
            throw new CommandParameterValidationException(validationResult);
    }

    /// <inheritdoc/>
    public async Task<int> ExecuteAsync(CommandContext context, CancellationToken cancellationToken = default)
    {
        // go!
        return await _pipeline(context, cancellationToken).ConfigureAwait(false);
    }

    private static IEnumerable<(AbstractCommandParameter Parameter, string Value)> GetTokensByParameter(IEnumerable<CommandToken> tokens)
    {
        foreach (var token in tokens)
        {
            switch (token.TokenType)
            {
                case CommandTokenType.Argument when token.Context is CommandArgument argument:
                    yield return (argument, token.Value);
                    break;

                case CommandTokenType.Option when token.Context is CommandOption option && option.IsFlag:
                    yield return (option, Boolean.TrueString);
                    break;

                case CommandTokenType.OptionGroup when token.Context is IEnumerable<CommandOption> optionGroup:
                    foreach (var option in optionGroup)
                        yield return (option, Boolean.TrueString);
                    break;

                case CommandTokenType.OptionValue when token.Context is CommandOption option:
                    yield return (option, token.Value);
                    break;

                case CommandTokenType.Unknown:
                    yield return (Builtins.Arguments.Remaining, token.Value);
                    break;
            }
        }
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
