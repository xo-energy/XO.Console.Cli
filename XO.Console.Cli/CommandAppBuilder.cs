using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using XO.Console.Cli.Implementation;
using XO.Console.Cli.Infrastructure;
using XO.Console.Cli.Middleware;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// Configures a command-line application.
/// </summary>
public sealed class CommandAppBuilder : ICommandAppBuilder
{
    private const string RootVerb = "__ROOT__";

    private readonly CommandBuilder _commandBuilder;
    private readonly ImmutableDictionary<Type, ParameterValueConverter>.Builder _converters;
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

    /// <summary>
    /// Initializes a new instance of <see cref="CommandAppBuilder"/>.
    /// </summary>
    public CommandAppBuilder()
        : this(CommandBuilder.CreateMissing(RootVerb, typeof(CommandParameters))) { }

    /// <summary>
    /// Initializes a new instance of <see cref="CommandAppBuilder"/>.
    /// </summary>
    /// <param name="rootCommandBuilder">An <see cref="ICommandBuilder"/> that configures the root (default) command.</param>
    private CommandAppBuilder(CommandBuilder rootCommandBuilder)
    {
        _commandBuilder = rootCommandBuilder;
        _converters = ImmutableDictionary.CreateBuilder<Type, ParameterValueConverter>();
        _globalOptions = ImmutableList.CreateBuilder<CommandOption>();
        _middleware = new List<Func<ExecutorDelegate, ExecutorDelegate>>(0);
        _entryAssembly = Assembly.GetEntryAssembly();

        // set default values of configurable settings
        foreach (var converter in CommandAppDefaults.Converters)
            _converters.Add(converter.ValueType, converter);
        _optionStyle = CommandAppDefaults.OptionStyle;
        _strict = CommandAppDefaults.Strict;
        _resolver = DefaultTypeResolver.Instance;

        // set default root command description from assembly description, if present
        var descriptionAttribute = _entryAssembly?.GetCustomAttribute<AssemblyDescriptionAttribute>();
        if (descriptionAttribute != null)
            _commandBuilder.SetDescription(descriptionAttribute.Description);

        // call generated configuration methods
        TypeRegistry.ConfigureCommandApp(this);
    }

    /// <summary>
    /// Creates a new instance of <see cref="CommandAppBuilder"/> with a default command.
    /// </summary>
    /// <remarks>The default command is invoked when the command-line arguments do not specify a sub-command.</remarks>
    /// <typeparam name="TCommand">The command implementation type.</typeparam>
    public static CommandAppBuilder WithDefaultCommand<TCommand>()
        where TCommand : class, ICommand
    {
        var rootCommandBuilder = TypeRegistry.CreateCommandBuilder<TCommand>(RootVerb);
        return new CommandAppBuilder(rootCommandBuilder);
    }

    /// <summary>
    /// Creates a new instance of <see cref="CommandAppBuilder"/> with a default command.
    /// </summary>
    /// <remarks>The default command is invoked when the command-line arguments do not specify a sub-command.</remarks>
    /// <param name="executeAsync">The command implementation delegate.</param>
    public static CommandAppBuilder WithDefaultCommand(
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync)
    {
        var rootCommandBuilder = CommandBuilder.CreateDelegate(RootVerb, executeAsync, typeof(CommandParameters));
        return new CommandAppBuilder(rootCommandBuilder);
    }

    /// <summary>
    /// Creates a new instance of <see cref="CommandAppBuilder"/> with a default command.
    /// </summary>
    /// <remarks>The default command is invoked when the command-line arguments do not specify a sub-command.</remarks>
    /// <param name="executeAsync">The command implementation delegate.</param>
    /// <typeparam name="TParameters">A class whose properties describe the command parameters.</typeparam>
    public static CommandAppBuilder WithDefaultCommand<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync)
        where TParameters : CommandParameters
    {
        var rootCommandBuilder = CommandBuilder.CreateDelegate<TParameters>(RootVerb, executeAsync);
        return new CommandAppBuilder(rootCommandBuilder);
    }

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
    public ICommandAppBuilder AddBranch(string name, Action<ICommandBuilder> configure)
    {
        _commandBuilder.AddBranch(name, configure);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder AddBranch<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string name,
        Action<ICommandBuilder> configure)
        where TParameters : CommandParameters
    {
        _commandBuilder.AddBranch<TParameters>(name, configure);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder AddCommand<TCommand>(
        string verb,
        Action<ICommandBuilder>? configure = null)
        where TCommand : class, ICommand
    {
        _commandBuilder.AddCommand<TCommand>(verb, configure);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder AddDelegate(
        string verb,
        Func<ICommandContext, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
    {
        _commandBuilder.AddDelegate(verb, executeAsync, configure);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder AddDelegate<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TParameters>(
        string verb,
        Func<ICommandContext, TParameters, CancellationToken, Task<int>> executeAsync,
        Action<ICommandBuilder>? configure = null)
        where TParameters : CommandParameters
    {
        _commandBuilder.AddDelegate(verb, executeAsync, configure);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder AddGlobalOption(
        string name,
        string description,
        params string[] aliases)
    {
        var option = new CommandOption(
            name,
            (context, values, _) => context.SetGlobalOption(name, values.ToImmutableArray()),
            typeof(string),
            description: description)
        {
            Aliases = ImmutableArray.Create(aliases),
        };

        _globalOptions.Add(option);
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder AddParameterConverter<TValue>(Func<string, TValue> converter)
    {
        _converters[typeof(TValue)] = new ParameterValueConverter<TValue>(converter);
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
    public ICommandAppBuilder UseMiddleware(ICommandAppMiddleware middleware)
    {
        _middleware.Add((next) =>
        {
            var adapter = new MiddlewareAdapter(middleware, next);

            return adapter.ExecuteAsync;
        });
        return this;
    }

    /// <inheritdoc/>
    public ICommandAppBuilder UseMiddleware<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TMiddleware>()
        where TMiddleware : ICommandAppMiddleware
    {
        _middleware.Add((next) =>
        {
            var middleware = _resolver.Get<TMiddleware>()
                ?? throw new InvalidOperationException($"Could not create instance of middleware type {typeof(TMiddleware)}");
            var adapter = new MiddlewareAdapter(middleware, next);

            return adapter.ExecuteAsync;
        });
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
