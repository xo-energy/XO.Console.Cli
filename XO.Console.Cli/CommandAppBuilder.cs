using System.Collections.Immutable;
using System.Reflection;
using XO.Console.Cli.Middleware;

namespace XO.Console.Cli;

/// <summary>
/// Configures a command-line application.
/// </summary>
/// <remarks>
/// Use the static factory methods to create an instance of this class.
/// </remarks>
public sealed class CommandAppBuilder : ICommandAppBuilder
{
    private const string ExecuteMethodName = "Execute";
    private const string ExecuteAsyncMethodName = "ExecuteAsync";
    private const string RootVerb = "__ROOT__";

    private readonly CommandBuilder _commandBuilder;
    private readonly ImmutableDictionary<Type, Func<string, object?>>.Builder _converters;
    private readonly ImmutableList<CommandOption>.Builder _globalOptions;
    private readonly List<Func<ExecutorDelegate, ExecutorDelegate>> _middleware;
    private readonly Assembly? _entryAssembly;

    private string? _applicationName;
    private string? _applicationVersion;
    private bool? _optionLeaderMustStartOption;
    private StringComparer? _optionNameComparer;
    private CommandOptionStyle _optionStyle;
    private char? _optionValueSeparator;
    private bool _strict;
    private ITypeResolver _resolver;
    private bool _useExceptionHandler;

    private CommandAppBuilder(Type parametersType, CommandFactory? commandFactory = null)
    {
        _commandBuilder = new CommandBuilder(RootVerb, parametersType, commandFactory);
        _converters = ImmutableDictionary.CreateBuilder<Type, Func<string, object?>>();
        _globalOptions = ImmutableList.CreateBuilder<CommandOption>();
        _middleware = new List<Func<ExecutorDelegate, ExecutorDelegate>>(0);
        _entryAssembly = Assembly.GetEntryAssembly();

        // set default values of configurable settings
        _converters.AddRange(CommandAppDefaults.Converters);
        _optionStyle = CommandAppDefaults.OptionStyle;
        _strict = CommandAppDefaults.Strict;
        _resolver = DefaultTypeResolver.Instance;

        // set default root command description from assembly description, if present
        var descriptionAttribute = _entryAssembly?.GetCustomAttribute<AssemblyDescriptionAttribute>();
        if (descriptionAttribute != null)
            _commandBuilder.SetDescription(descriptionAttribute.Description);
    }

    /// <summary>
    /// Creates a new instance of <see cref="ICommandAppBuilder"/>.
    /// </summary>
    public static ICommandAppBuilder Create()
        => new CommandAppBuilder(typeof(CommandParameters));

    /// <summary>
    /// Creates a new instance of <see cref="ICommandAppBuilder"/> with a default command.
    /// </summary>
    /// <remarks>The default command is invoked when the command-line arguments do not specify a sub-command.</remarks>
    /// <typeparam name="TCommand">The command implementation type.</typeparam>
    public static ICommandAppBuilder WithDefaultCommand<TCommand>()
        where TCommand : class, ICommand
    {
        return new CommandAppBuilder(
            CommandBuilder.GetParametersType<TCommand>(),
            CommandBuilder.CreateCommandFactory<TCommand>());
    }

    /// <summary>
    /// Creates a new instance of <see cref="ICommandAppBuilder"/> with a default command.
    /// </summary>
    /// <remarks>The default command is invoked when the command-line arguments do not specify a sub-command.</remarks>
    /// <param name="executeAsync">The command implementation delegate.</param>
    /// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
    public static ICommandAppBuilder WithDefaultCommand<TParameters>(
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
    {
        return new CommandAppBuilder(
            typeof(TParameters),
            CommandBuilder.CreateCommandFactory(executeAsync));
    }

    /// <inheritdoc/>
    ICommandBuilder ICommandBuilderProvider<ICommandAppBuilder>.Builder
        => _commandBuilder;

    /// <inheritdoc/>
    ICommandAppBuilder ICommandBuilderProvider<ICommandAppBuilder>.Self
        => this;

    /// <inheritdoc/>
    public ICommandApp Build()
    {
        var rootCommand = _commandBuilder.Build();
        var pipeline = BuildPipeline();

        var settings = new CommandAppSettings(
            _applicationName ?? GetDefaultApplicationName(),
            _applicationVersion ?? GetDefaultApplicationVersion(),
            _converters.ToImmutable(),
            _globalOptions.ToImmutable())
        {
            OptionLeader = _optionStyle.GetLeader(),
            OptionLeaderMustStartOption = _optionLeaderMustStartOption ?? _optionStyle.GetDefaultLeaderMustStartOption(),
            OptionNameComparer = _optionNameComparer ?? _optionStyle.GetDefaultNameComparer(),
            OptionStyle = _optionStyle,
            OptionValueSeparator = _optionValueSeparator ?? _optionStyle.GetDefaultValueSeparator(),
            Strict = _strict,
        };

        return new CommandApp(_resolver, pipeline, rootCommand, settings);
    }

    /// <inheritdoc/>
    public ICommandAppBuilder AddGlobalOption<TValue>(
        string name,
        string description,
        params string[] aliases)
    {
        var option = new CommandOption(
            new CommandOptionAttribute(name, aliases),
            typeof(CommandAppBuilder),
            typeof(TValue),
            (context, value) => context.SetGlobalOption(name, value),
            description: description);

        _globalOptions.Add(option);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder AddParameterConverter<TValue>(Func<string, TValue?> converter)
    {
        _converters[typeof(TValue)] = (value) => converter(value);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder DisableStrictParsing()
    {
        _strict = false;
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder SetApplicationName(string name)
    {
        _applicationName = name;
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder SetApplicationVersion(string version)
    {
        _applicationVersion = version;
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder SetDescription(string description)
    {
        _commandBuilder.SetDescription(description);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder SetOptionStyle(
        CommandOptionStyle style,
        bool? optionLeaderMustStartOption = default,
        StringComparer? optionNameComparer = default,
        char? optionValueSeparator = default)
    {
        _optionStyle = style;

        if (optionLeaderMustStartOption.HasValue)
            _optionLeaderMustStartOption = optionLeaderMustStartOption.Value;

        if (optionNameComparer != null)
            _optionNameComparer = optionNameComparer;

        if (optionValueSeparator.HasValue)
            _optionValueSeparator = optionValueSeparator.Value;

        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder UseExceptionHandler()
    {
        _useExceptionHandler = true;
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder UseMiddleware(Func<ExecutorDelegate, ExecutorDelegate> middleware)
    {
        _middleware.Add(middleware);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder UseMiddleware<TMiddleware>(params object[] args)
    {
        // find a suitable constructor
        ConstructorInfo constructor;
        try
        {
            constructor = typeof(TMiddleware)
                .GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .Single();
        }
        catch (Exception ex)
        {
            throw new CommandTypeException(
                typeof(TMiddleware),
                $"Middleware class must have exactly one public constructor",
                ex);
        }

        // find a suitable execute method
        MethodInfo executeMethod;
        try
        {
            executeMethod = typeof(TMiddleware)
                .GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Single(method => method.Name is ExecuteMethodName or ExecuteAsyncMethodName);
        }
        catch (Exception ex)
        {
            throw new CommandTypeException(
                typeof(TMiddleware),
                $"Middleware class must have exactly one public method named {ExecuteMethodName} or {ExecuteAsyncMethodName}",
                ex);
        }

        // validate the constructor parameters
        var constructorParameters = constructor.GetParameters();
        if (constructorParameters.Length < 1 ||
            constructorParameters[0].ParameterType != typeof(ExecutorDelegate))
        {
            throw new CommandTypeException(
                typeof(TMiddleware),
                $"Constructor must have a first parameter of type '{typeof(ExecutorDelegate)}'");
        }

        // validate the method parameters
        var executeMethodParameters = executeMethod.GetParameters();
        if (executeMethodParameters.Length < 1 ||
            executeMethodParameters[0].ParameterType != typeof(CommandContext))
        {
            throw new CommandTypeException(
                typeof(TMiddleware),
                $"{executeMethod} must have a first parameter of type '{typeof(CommandContext)}'");
        }
        if (executeMethodParameters.Length < 2 ||
            executeMethodParameters[^1].ParameterType != typeof(CancellationToken))
        {
            throw new CommandTypeException(
                typeof(TMiddleware),
                $"{executeMethod} must have a last parameter of type '{typeof(CancellationToken)}'");
        }
        if (executeMethod.ReturnType != typeof(Task<int>))
        {
            throw new CommandTypeException(
                typeof(TMiddleware),
                $"{executeMethod} must have a return type of '{typeof(Task<int>)}'");
        }

        var factory = CreateMiddlewareFactory(
            constructor,
            executeMethod,
            executeMethodParameters,
            args);

        _middleware.Add(factory);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder UseTypeResolver(ITypeResolver resolver)
    {
        _resolver = resolver;
        return this;
    }

    private ExecutorDelegate BuildPipeline()
    {
        ExecutorDelegate pipeline = (context, cancellationToken) =>
        {
            return context.Command.ExecuteAsync(
                context,
                context.Parameters,
                cancellationToken);
        };

        foreach (var middleware in Enumerable.Reverse(_middleware))
            pipeline = middleware(pipeline);

        if (_useExceptionHandler)
            pipeline = new ExceptionHandlerMiddleware(pipeline).ExecuteAsync;

        return pipeline;
    }

    private Func<ExecutorDelegate, ExecutorDelegate> CreateMiddlewareFactory(
        ConstructorInfo constructor,
        MethodInfo executeMethod,
        ParameterInfo[] executeMethodParameters,
        object[] args)
    {
        return next =>
        {
            var parameters = constructor.GetParameters();
            var parameterValues = new object?[parameters.Length];
            var argsIndex = 0;

            // first parameter must be of type ExecutorDelegate
            parameterValues[0] = next;

            // assign values from args in order by parameter type, populating the rest from the resolver
            for (int i = 1; i < parameters.Length; ++i)
            {
                if (argsIndex < args.Length && parameters[i].ParameterType == args[argsIndex].GetType())
                    parameterValues[i] = args[argsIndex++];
                else
                    parameterValues[i] = _resolver.Get(parameters[i].ParameterType);
            }

            // construct the middleware
            var middleware = constructor.Invoke(parameterValues);

            // if the execute method's signature matches, just return it as a bound delegate
            if (executeMethodParameters.Length == 2)
                return executeMethod.CreateDelegate<ExecutorDelegate>(middleware);

            // otherwise, construct a delegate the hard way
            return (context, cancellationToken) =>
            {
                var executeArgs = new object?[executeMethodParameters.Length];

                executeArgs[0] = context;
                executeArgs[^1] = cancellationToken;

                for (int i = 1; i < executeMethodParameters.Length - 1; ++i)
                    executeArgs[i] = _resolver.Get(executeMethodParameters[i].ParameterType);

                return (Task<int>)executeMethod.Invoke(middleware, executeArgs)!;
            };
        };
    }

    private string GetDefaultApplicationName()
    {
        var name = _entryAssembly?.GetName().Name;

        if (name == null && Environment.ProcessPath != null)
            name = Path.GetFileName(Environment.ProcessPath);

        return name ?? String.Empty;
    }

    private string GetDefaultApplicationVersion()
    {
        // try to use the informational version attribute on the assembly
        var attr = _entryAssembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attr != null)
            return attr.InformationalVersion;

        // if that doesn't work, use the assembly version
        return _entryAssembly?.GetName().Version?.ToString()
            ?? String.Empty;
    }
}
