using System.Collections.Immutable;

namespace XO.Console.Cli;

/// <summary>
/// Defines default values for <see cref="ICommandAppBuilder"/> settings.
/// </summary>
public static class CommandAppDefaults
{
    /// <summary>
    /// Gets the collection of default parameter value converters.
    /// </summary>
    public static IReadOnlyDictionary<Type, Func<string, object?>> Converters
        = ImmutableDictionary<Type, Func<string, object?>>.Empty;

    /// <summary>
    /// Gets the default option style, which determines the required leading character(s) in options names.
    /// </summary>
    public const CommandOptionStyle OptionStyle = CommandOptionStyle.Posix;

    /// <summary>
    /// Gets whether command parsing is strict by default (i.e. whether to throw an exception for unknown or missing
    /// arguments).
    /// </summary>
    public const bool Strict = true;
}
