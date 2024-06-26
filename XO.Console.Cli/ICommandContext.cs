using System.Collections.Immutable;
using XO.Console.Cli.Model;

namespace XO.Console.Cli;

/// <summary>
/// Provides read-only access to the <see cref="CommandContext"/> to the command implementation.
/// </summary>
public interface ICommandContext
{
    /// <summary>
    /// Gets the console abstraction. By default, this is a thin wrapper around the system console.
    /// </summary>
    /// <remarks>
    /// Middleware can replace the <see cref="IConsole"/> implementation for testing, redirection, or other
    /// processing.
    /// </remarks>
    IConsole Console { get; }

    /// <summary>
    /// Gets a list of argument values that could not be bound to a parameter.
    /// </summary>
    /// <remarks>
    /// This collection will be empty unless strict parsing is disabled.
    /// </remarks>
    List<string> RemainingArguments { get; }

    /// <summary>
    /// Gets the <see cref="CommandParseResult"/> to which this context is bound.
    /// </summary>
    CommandParseResult ParseResult { get; }

    /// <summary>
    /// Gets the value of a global option.
    /// </summary>
    /// <param name="name">The option name, including the option leader (prefix).</param>
    /// <returns>The option value.</returns>
    /// <exception cref="KeyNotFoundException">The specified global option is not set.</exception>
    ImmutableArray<string> GetGlobalOption(string name);

    /// <summary>
    /// Gets the value of a global option.
    /// </summary>
    /// <param name="name">The option name, including the option leader (prefix).</param>
    /// <param name="value">The option value, if the option is set.</param>
    /// <returns>If the option was set, <c>true</c>; otherwise, <c>false</c>.</returns>
    bool TryGetGlobalOption(string name, out ImmutableArray<string> value);
}
