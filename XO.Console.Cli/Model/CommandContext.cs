using System.Collections.Immutable;
using XO.Console.Cli.Features;

namespace XO.Console.Cli.Model;

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
    private Dictionary<string, ImmutableArray<string>>? _globalOptions;
    private List<string>? _remainingArguments;

    /// <summary>
    /// Creates a new instance of <see cref="CommandContext"/>.
    /// </summary>
    /// <param name="scope">The service scope for this command execution.</param>
    /// <param name="command">The command implementation.</param>
    /// <param name="parameters">The command parameters.</param>
    /// <param name="parseResult">The <see cref="CommandParseResult"/> to which this context is bound.</param>
    public CommandContext(
        ITypeResolverScope scope,
        ICommand command,
        CommandParameters parameters,
        CommandParseResult parseResult)
    {
        _scope = scope;
        this.CommandServices = scope.TypeResolver;
        this.Command = command;
        this.Parameters = parameters;
        this.ParseResult = parseResult;
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

    /// <summary>
    /// Gets or sets the <see cref="CommandParseResult"/> to which this context is bound.
    /// </summary>
    public CommandParseResult ParseResult { get; set; }

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
    public ImmutableArray<string> GetGlobalOption(string name)
        => TryGetGlobalOption(name, out var values) ? values : throw new KeyNotFoundException($"Global option '{name}' is not set");

    /// <summary>
    /// Sets the value of a global option.
    /// </summary>
    /// <param name="name">The option name, including the option leader (prefix).</param>
    /// <param name="values">The option values.</param>
    public void SetGlobalOption(string name, ImmutableArray<string> values)
    {
        LazyInitializer.EnsureInitialized(ref _globalOptions);
        _globalOptions[name] = values;
    }

    /// <inheritdoc/>
    public bool TryGetGlobalOption(string name, out ImmutableArray<string> values)
    {
        if (_globalOptions?.TryGetValue(name, out var valuesNonNull) == true)
        {
            values = valuesNonNull;
            return true;
        }
        else
        {
            values = default;
            return false;
        }
    }
}
