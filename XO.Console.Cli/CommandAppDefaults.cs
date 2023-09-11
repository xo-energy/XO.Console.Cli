using XO.Console.Cli.Infrastructure;

namespace XO.Console.Cli;

/// <summary>
/// Defines default values for <see cref="ICommandAppBuilder"/> settings.
/// </summary>
public static class CommandAppDefaults
{
    /// <summary>
    /// Gets the collection of default parameter value converters.
    /// </summary>
    public static readonly IReadOnlyList<ParameterValueConverter> Converters
        = new ParameterValueConverter[]
        {
            ParameterValueConverter.FromDelegate(static (value) => DateOnly.Parse(value)),
            ParameterValueConverter.FromDelegate(static (value) => DateTimeOffset.Parse(value)),
            ParameterValueConverter.FromDelegate(static (value) => new DirectoryInfo(value)),
            ParameterValueConverter.FromDelegate(static (value) => new FileInfo(value)),
            ParameterValueConverter.FromDelegate(static (value) => Guid.Parse(value)),
            ParameterValueConverter.FromDelegate(static (value) => TimeOnly.Parse(value)),
            ParameterValueConverter.FromDelegate(static (value) => TimeSpan.Parse(value)),
            ParameterValueConverter.FromDelegate(static (value) => new Uri(value)),
        };

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
