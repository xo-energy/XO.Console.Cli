using XO.Console.Cli.Features;

namespace XO.Console.Cli;

/// <summary>
/// Represents the context of a command execution.
/// </summary>
/// <remarks>
/// Middleware can modify the <see cref="CommandContext"/> before (or after) it gets passed to the command
/// implementation.
/// </remarks>
public sealed class CommandContext : IAsyncDisposable, ICommandContext, IDisposable
{
    private readonly ITypeResolverScope _scope;

    private IConsole? _console;
    private Dictionary<string, object?>? _globalOptions;
    private List<string>? _remainingArguments;

    /// <summary>
    /// Creates a new instance of <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="scope">The service scope for this command execution.</param>
    /// <param name="command">The command implementation.</param>
    /// <param name="parameters">The command parameters.</param>
    public CommandContext(
        ITypeResolverScope scope,
        ICommand command,
        CommandParameters parameters)
    {
        _scope = scope;
        this.CommandServices = scope.TypeResolver;
        this.Command = command;
        this.Parameters = parameters;
    }

    /// <summary>
    /// Gets or sets the <see cref="ITypeResolver"/> that resolves dependencies for this command execution.
    /// </summary>
    public ITypeResolver CommandServices { get; set; }

    /// <summary>
    /// Gets or sets the command implementation.
    /// </summary>
    public ICommand Command { get; set; }

    /// <summary>
    /// Gets or sets the command parameters.
    /// </summary>
    public CommandParameters Parameters { get; set; }

    /// <inheritdoc/>
    /// <summary>Gets or sets the console abstraction.</summary>
    public IConsole Console
    {
        get => LazyInitializer.EnsureInitialized(ref _console, () => new SystemConsole());
        set => _console = value;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        _scope.Dispose();
    }

    /// <inheritdoc/>
    public ValueTask DisposeAsync()
    {
        return _scope.DisposeAsync();
    }

    /// <inheritdoc/>
    public List<string> RemainingArguments
        => LazyInitializer.EnsureInitialized(ref _remainingArguments);

    /// <inheritdoc/>
    public TValue? GetGlobalOption<TValue>(string name)
        => TryGetGlobalOption(name, out TValue? value) ? value : default;

    /// <summary>
    /// Sets the value of a global option.
    /// </summary>
    /// <param name="name">The option name, including the option leader (prefix).</param>
    /// <param name="value">The option value.</param>
    /// <typeparam name="TValue">The type of the option's value.</typeparam>
    public void SetGlobalOption<TValue>(string name, TValue value)
    {
        LazyInitializer.EnsureInitialized(ref _globalOptions);
        _globalOptions[name] = value;
    }

    /// <inheritdoc/>
    public bool TryGetGlobalOption<TValue>(string name, out TValue? value)
    {
        if (_globalOptions?.TryGetValue(name, out var objectValue) == true)
        {
            value = (TValue?)objectValue;
            return true;
        }
        else
        {
            value = default;
            return false;
        }
    }
}
